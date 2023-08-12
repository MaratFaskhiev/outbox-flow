# OutboxFlow [![build and test](https://github.com/MaratFaskhiev/outbox-flow/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/MaratFaskhiev/outbox-flow/actions/workflows/build-and-test.yml)

## Introduction
Simple implementation of the transactional outbox based on the pipeline pattern.

**Disclaimer**

This project is a pet project which I'm going to support in a free time. It can be used as an example of how outbox pattern can be implemented in an extensible way.

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

This code registers a background service that reads and sends outbox messages to the specified destination.

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

### To Be Done :)

* Unit tests (!)
* Documentation
* Entity Framework support
* OpenTelemetry support
* and more...