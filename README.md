# OutboxFlow [![build and test](https://github.com/MaratFaskhiev/outbox-flow/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/MaratFaskhiev/outbox-flow/actions/workflows/build-and-test.yml)

## Introduction

Simple implementation of the transactional outbox based on the pipeline pattern.

**Disclaimer**

This project is a pet project which I'm going to support in a free time. It can be used as an example of how outbox pattern can be implemented in an extensible way.

## Documentation

| Document | Description | Audience |
|---|---|---|
| [Architecture](docs/architecture.md) | Pipeline pattern, contexts, message lifecycle | All |
| [Getting Started](docs/getting-started.md) | Step-by-step setup and first run | New users |
| [Configuration](docs/configuration.md) | Full fluent API reference | Developers |
| [Middleware](docs/middleware.md) | Custom middleware development guide | Developers |
| [Storage](docs/storage.md) | Storage provider implementation guide | Contributors |
| [Kafka](docs/kafka.md) | Kafka destination configuration | Developers |
| [Serialization](docs/serialization.md) | Serializer guide (JSON, Protobuf, custom) | Developers |

## Quick Start

Install the required NuGet packages:

```shell
dotnet add package OutboxFlow
dotnet add package OutboxFlow.Postgres
dotnet add package OutboxFlow.Kafka
```

Configure the outbox in your `Program.cs`:

```csharp
services
    .AddKafka()
    .AddOutbox(outboxBuilder =>
        outboxBuilder
            .AddProducer(producer => producer
                .UsePostgres()
                .ForMessage<MyMessage>(pipeline =>
                    pipeline
                        .SerializeWithJson()
                        .SetDestination("my-topic")
                        .Save()
                )
            )
            .AddConsumer(consumer =>
                consumer
                    .UsePostgres(connectionString)
                    .SetDefaultRoute(pipeline => pipeline.SendToKafka(producerConfig))
            )
    );
```

Produce a message within a database transaction:

```csharp
await _producer.ProduceAsync(new MyMessage("Hello!"), transaction, cancellationToken);
```

The consumer background service reads messages from the outbox storage and sends them to Kafka automatically.

## Packages

| Package | Description |
|---|---|
| [OutboxFlow](https://www.nuget.org/packages/OutboxFlow/) | Core library: pipeline, produce, consume, storage abstractions |
| [OutboxFlow.Postgres](https://www.nuget.org/packages/OutboxFlow.Postgres/) | PostgreSQL outbox storage implementation |
| [OutboxFlow.Kafka](https://www.nuget.org/packages/OutboxFlow.Kafka/) | Kafka outbox destination implementation |

## Overview

OutboxFlow provides two base abstractions: producers and consumers.

### Producers

The purpose of producers is to save messages to an outbox storage.

Let's check the sample producer configuration:

```csharp
services
    // Register the outbox dependencies
    .AddOutbox(outboxBuilder =>
        outboxBuilder
            // Register the producer dependencies
            .AddProducer(producer => producer
                // Use PostgreSQL as an underlying storage
                .UsePostgres()
                // Configure pipeline for the SampleTextModel message type
                .ForMessage<SampleTextModel>(pipeline =>
                    pipeline
                        // Add sample synchronous middleware
                        .AddSyncStep<LoggingMiddleware, SampleTextModel>()
                        // Convert message to the prototype model
                        .AddSyncStep((message, _) => new Protos.SampleTextModel
                        {
                            Value = message.Value
                        })
                        // Serialize the prototype model to a byte array
                        .SerializeWithProtobuf()
                        // Set the message destination
                        .SetDestination("topic")
                        // Save the message to a storage
                        .Save()
                )
            )
        );
```

In this example, we configure a produce pipeline for the `SampleTextModel` message type.

Now we are ready to produce a first message:

```csharp
private readonly IProducer _producer;
private readonly string _connectionString;

private async Task ProduceSampleMessageAsync(CancellationToken cancellationToken)
{
    await using var connection = new NpgsqlConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    await using var transaction = await connection.BeginTransactionAsync(
        IsolationLevel.ReadCommitted, cancellationToken);

    await _producer.ProduceAsync(
        new SampleTextModel("Hello world!"),
        transaction,
        cancellationToken);

    await transaction.CommitAsync(cancellationToken);
}
```

For high-throughput scenarios, produce multiple messages in a single batch by registering a pipeline for a collection type:

```csharp
// Configuration — register a pipeline for a collection type
.ForMessage<IReadOnlyCollection<SampleTextModel>>(pipeline =>
    pipeline
        .ForEach<SampleTextModel>(sub =>
        {
            sub.AddSyncStep((message, _) => new Protos.SampleTextModel
            {
                Value = message.Value
            })
            .SerializeWithProtobuf()
            .SetDestination("topic");
        })
        .SaveBatch()
)

// Usage — simply call ProduceAsync with the collection
private async Task ProduceBatchSampleAsync(CancellationToken cancellationToken)
{
    await using var connection = new NpgsqlConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    await using var transaction = await connection.BeginTransactionAsync(
        IsolationLevel.ReadCommitted, cancellationToken);

    var messages = new[]
    {
        new SampleTextModel("Message 1"),
        new SampleTextModel("Message 2"),
        new SampleTextModel("Message 3")
    };

    await _producer.ProduceAsync(
        messages,
        transaction,
        cancellationToken);

    await transaction.CommitAsync(cancellationToken);
}
```

Each message runs through the configured pipeline (serialization, destination, key). The `ForEach` step runs the sub-pipeline for each item and collects the resulting produce contexts. The `SaveBatch` step persists all messages using a single storage operation when the storage supports it (e.g., PostgreSQL `NpgsqlBatch`).

### Consumers

The purpose of consumers is to read messages from outbox storage and send them to the destination.

Let's check the sample consumer configuration:

```csharp

var producerConfig = new ProducerConfig
{
    BootstrapServers = "localhost:9092"
};

services
    // Register Apache Kafka dependencies
    .AddKafka()
    // Register the outbox dependencies
    .AddOutbox(outboxBuilder =>
        outboxBuilder
            // Register the consumer dependencies
            .AddConsumer(consumer =>
                consumer
                    // Use PostgreSQL as an underlying storage
                    .UsePostgres(context.Configuration.GetConnectionString("Postgres")!)
                    // Configure the default pipeline for outbox messages.
                    // Default route will be used for all destinations which are not configured explicitly
                    .SetDefaultRoute(pipeline => pipeline.SendToKafka(producerConfig))
            )
        );
```

This code registers a background service that reads and sends outbox messages to the specified destination. `SendToKafka` method tells that it is necessary to send a message to a Kafka topic by using the specified producer configuration.

What should we do to configure Apache Kafka producer?
* Implement `IKafkaProducerBuilder` interface. [DefaultKafkaProducerBuilder](src/OutboxFlow.Kafka/DefaultKafkaProducerBuilder.cs) can be used as an example
* Register `IKafkaProducerBuilder` implementation
* Pass the implementation type as a type parameter to the `SendToKafka` method

Now let's get familiar with the underlying message storage.

### Outbox storage

As you noticed, we used the `UsePostgres` method. It indicates that we are going to use PostgreSQL implementation of the outbox storage. But first we need to create all required tables for this implementation. Let's check the script:

```sql
create table if not exists outbox_state
(
    id        uuid default gen_random_uuid() not null primary key,
    expire_at timestamptz                    not null
);

create table if not exists outbox_message
(
    id          bigint generated by default as identity,
    destination text,
    headers     jsonb,
    key         bytea,
    value       bytea       not null,
    created_at  timestamptz not null
);
```

We are creating two tables:
* `outbox_message` is used to store messages
* `outbox_state` is used to sync access from multiple consumers. The single consumer is allowed to read messages at a time

### Sample

You can check the complete sample application [here](samples/OutboxFlow.Sample).

### To Be Done

* Entity Framework support
* OpenTelemetry support
* and more...