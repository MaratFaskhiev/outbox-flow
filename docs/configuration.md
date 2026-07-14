# Configuration Reference

## IOutboxBuilder

The entry point for configuring OutboxFlow. Obtain it via the `AddOutbox` extension method on `IServiceCollection`.

| Method | Description |
|---|---|
| `AddProducer(Action<IProducerBuilder> configure)` | Configures produce pipelines for one or more message types. |
| `AddConsumer(Action<IConsumerBuilder> configure)` | Configures consume pipelines. |
| `Build(IServiceCollection services)` | Builds all outbox pipelines and registers services. |

```csharp
services.AddOutbox(outbox =>
    outbox
        .AddProducer(producer => { /* ... */ })
        .AddConsumer(consumer => { /* ... */ })
);
```

## Producer Configuration

### IProducerBuilder

The producer builder is obtained inside the `AddProducer` callback.

| Property / Method | Description |
|---|---|---|
| `IOutboxStorageRegistrar? OutboxStorageRegistrar { get; set; }` | Storage registrar (set by `UsePostgres()` or a custom registrar). |
| `ForMessage<T>(Action<IProducePipelineBuilder<T>> configure)` | Configures the produce pipeline for message type `T`. |
| `Build(IServiceCollection services)` | Builds the producer and registers services. |

### IProducePipelineBuilder&lt;T&gt;

A marker interface extending `IProducePipelineStepBuilder<T, T>`. It represents the start of a produce pipeline for a specific message type.

```csharp
producer.ForMessage<MyMessage>(pipeline =>
    pipeline
        .AddSyncStep<LoggingMiddleware, MyMessage>()
        .SerializeWithJson()
        .SetDestination("my-topic")
        .Save()
);
```

### IProducePipelineStepBuilder&lt;TIn, TOut&gt;

Represents a step in the produce pipeline with input type `TIn` and output type `TOut`.

| Method | Return Type | Description |
|---|---|---|
| `AddSyncStep<TNext>(Func<TOut, IProduceContext, TNext> action)` | `IProducePipelineStepBuilder<TOut, TNext>` | Adds an inline synchronous step. |
| `AddAsyncStep<TNext>(Func<TOut, IProduceContext, ValueTask<TNext>> action)` | `IProducePipelineStepBuilder<TOut, TNext>` | Adds an inline asynchronous step. |

### Produce Extension Methods

All methods in `ProducePipelineStepBuilderExtensions` (adds middleware, sets destination, saves).

| Method | Return Type | Parameters | Description |
|---|---|---|---|
| `AddSyncStep<TMiddleware, T>()` | `IProducePipelineStepBuilder<T, T>` | — | Adds a synchronous middleware (`IProduceSyncMiddleware<T, T>`) that preserves the message type. |
| `AddSyncStep<TMiddleware, TIn, TOut>()` | `IProducePipelineStepBuilder<TOut, TOut>` | — | Adds a synchronous middleware that transforms `TIn` to `TOut`. |
| `AddSyncStep<TMiddleware, TIn, TOut, TNext>()` | `IProducePipelineStepBuilder<TOut, TNext>` | — | Adds a synchronous middleware that transforms `TIn` → `TOut`, pipeline continues as `TOut` → `TNext`. |
| `AddAsyncStep<TMiddleware, T>()` | `IProducePipelineStepBuilder<T, T>` | — | Adds an asynchronous middleware (`IProduceAsyncMiddleware<T, T>`) that preserves the message type. |
| `AddAsyncStep<TMiddleware, TIn, TOut>()` | `IProducePipelineStepBuilder<TOut, TOut>` | — | Adds an asynchronous middleware that transforms `TIn` to `TOut`. |
| `AddAsyncStep<TMiddleware, TIn, TOut, TNext>()` | `IProducePipelineStepBuilder<TOut, TNext>` | — | Adds an asynchronous middleware that transforms `TIn` → `TOut`, pipeline continues as `TOut` → `TNext`. |
| `SetKey<TIn, TOut>(Func<TOut, byte[]> keyProvider)` | `IProducePipelineStepBuilder<TOut, TOut>` | `keyProvider`: a function that extracts the message key from the message. | Sets the message key. |
| `SetDestination<TIn, TOut>(string destination)` | `IProducePipelineStepBuilder<TOut, TOut>` | `destination`: the topic or queue name. | Sets the message destination. |
| `Save<TIn, TOut>()` | `IProducePipelineStepBuilder<TOut, TOut>` | — | Saves the message to the outbox storage. Must be the **last step** in the producer pipeline. |

### IProducer

The `IProducer` interface is the entry point for producing outbox messages. Obtain it via DI after configuring the producer builder.

| Method | Description |
|---|---|
| `ProduceAsync<T>(T message, IDbTransaction transaction, CancellationToken)` | Produces an outbox message. The configured pipeline (including `Save()` or `SaveBatch()`) runs synchronously within the provided transaction. For batch producing, pass `IReadOnlyCollection<TItem>` as `T` and configure the pipeline with `ForEach<TItem>()` + `SaveBatch()`. |

## Consumer Configuration

### IConsumerBuilder

The consumer builder is obtained inside the `AddConsumer` callback.

| Property / Method | Description |
|---|---|
| `IOutboxStorageRegistrar? OutboxStorageRegistrar { get; set; }` | Storage registrar (set by `UsePostgres()` or a custom registrar). |
| `int BatchSize { get; set; }` | Number of messages to consume per batch. |
| `TimeSpan ConsumeDelay { get; set; }` | Delay between consume attempts. |
| `IsolationLevel IsolationLevel { get; set; }` | Transaction isolation level. |
| `TimeSpan Timeout { get; set; }` | Consume operation timeout. |
| `SetDefaultRoute(Action<IConsumePipelineBuilder> configure)` | Configures the default consume pipeline for all destinations. |
| `AddRoute(string destination, Action<IConsumePipelineBuilder> configure)` | Configures a destination-specific consume pipeline. |
| `Build(IServiceCollection services)` | Builds the consumer and registers services. |

### Consumer Builder Extension Methods

From `ConsumerBuilderExtensions`:

| Method | Return Type | Parameters | Description |
|---|---|---|---|
| `SetOutboxStorageRegistrar` | `IConsumerBuilder` | `IOutboxStorageRegistrar registrar` | Sets the outbox storage registrar. |
| `SetBatchSize` | `IConsumerBuilder` | `int batchSize` | Sets the amount of messages to consume per batch. |
| `SetConsumeDelay` | `IConsumerBuilder` | `TimeSpan consumeDelay` | Sets the delay between each consume attempt. |
| `SetIsolationLevel` | `IConsumerBuilder` | `IsolationLevel isolationLevel` | Sets the transaction isolation level. |
| `SetTimeout` | `IConsumerBuilder` | `TimeSpan timeout` | Sets the consume operation timeout. |

### IConsumePipelineBuilder

A marker interface extending `IConsumePipelineStepBuilder<IOutboxMessage, IOutboxMessage>`. Represents the start of a consume pipeline.

```csharp
consumer
    .SetDefaultRoute(pipeline => pipeline.SendToKafka(producerConfig))
    .AddRoute("specific-topic", pipeline =>
        pipeline
            .AddSyncStep<MyCustomMiddleware, IOutboxMessage, IOutboxMessage>()
            .SendToKafka(producerConfig)
    );
```

### IConsumePipelineStepBuilder&lt;TIn, TOut&gt;

Represents a step in the consume pipeline with input type `TIn` and output type `TOut`.

| Method | Return Type | Description |
|---|---|---|
| `AddSyncStep<TNext>(Func<TOut, IConsumeContext, TNext> action)` | `IConsumePipelineStepBuilder<TOut, TNext>` | Adds an inline synchronous step. |
| `AddAsyncStep<TNext>(Func<TOut, IConsumeContext, ValueTask<TNext>> action)` | `IConsumePipelineStepBuilder<TOut, TNext>` | Adds an inline asynchronous step. |

### Consume Extension Methods

From `ConsumePipelineStepBuilderExtensions`:

| Method | Return Type | Parameters | Description |
|---|---|---|---|
| `AddSyncStep<TMiddleware, TIn, TOut>()` | `IConsumePipelineStepBuilder<TOut, TOut>` | — | Adds a synchronous middleware that transforms `TIn` → `TOut`. |
| `AddSyncStep<TMiddleware, TIn, TOut, TNext>()` | `IConsumePipelineStepBuilder<TOut, TNext>` | — | Adds a synchronous middleware, pipeline continues as `TOut` → `TNext`. |
| `AddAsyncStep<TMiddleware, TIn, TOut>()` | `IConsumePipelineStepBuilder<TOut, TOut>` | — | Adds an asynchronous middleware that transforms `TIn` → `TOut`. |
| `AddAsyncStep<TMiddleware, TIn, TOut, TNext>()` | `IConsumePipelineStepBuilder<TOut, TNext>` | — | Adds an asynchronous middleware, pipeline continues as `TOut` → `TNext`. |

## Postgres Extensions

### Producer

```csharp
public static IProducerBuilder UsePostgres(this IProducerBuilder builder)
```

Configures the producer to use PostgreSQL-based outbox storage. Call this before `ForMessage<T>()`.

### Consumer

```csharp
public static IConsumerBuilder UsePostgres(this IConsumerBuilder builder, string connectionString)
```

Configures the consumer to use PostgreSQL-based outbox storage with the given connection string.

### Example

```csharp
outbox
    .AddProducer(producer => producer
        .UsePostgres()
        .ForMessage<MyMessage>(pipeline => /* ... */)
    )
    .AddConsumer(consumer => consumer
        .UsePostgres(connectionString)
        .SetDefaultRoute(pipeline => pipeline.SendToKafka(producerConfig))
    );
```

## Kafka Extensions

### Service Registration

```csharp
public static IServiceCollection AddKafka(this IServiceCollection services)
```

Registers Apache Kafka dependencies (required before `AddOutbox`).

### Send to Kafka

From `ConsumePipelineStepBuilderExtensions` (Kafka):

| Method | Return Type | Parameters | Description |
|---|---|---|---|
| `SendToKafka<TIn>()` | `IConsumePipelineStepBuilder<IOutboxMessage, IOutboxMessage>` | `ProducerConfig producerConfig` | Sends the message to Kafka using `DefaultKafkaProducerBuilder`. |
| `SendToKafka<TIn, TKafkaProducerBuilder>()` | `IConsumePipelineStepBuilder<IOutboxMessage, IOutboxMessage>` | `ProducerConfig producerConfig` | Sends the message to Kafka using a custom `IKafkaProducerBuilder` implementation. |

### Example

```csharp
// Default producer builder
pipeline.SendToKafka(producerConfig);

// Custom producer builder
pipeline.SendToKafka<MyProducerBuilder>(producerConfig);
```

## Serialization Extensions

All serialization extension methods are in the `ProducePipelineStepBuilderExtensions` partial class. They are called on the produce pipeline.

### Generic Serializer

| Method | Return Type | Parameters | Constraints | Description |
|---|---|---|---|---|
| `Serialize<TSerializer, TIn, TOut>()` | `IProducePipelineStepBuilder<TOut, TOut>` | — | `TSerializer : ISerializer<byte[]>` | Serialize a message using the specified serializer type. |
| `SerializeKey<TSerializer, TIn, TOut, TKey>()` | `IProducePipelineStepBuilder<TOut, TOut>` | `Func<TOut, TKey> keyProvider` | `TSerializer : ISerializer<byte[]>` | Serialize a message key using the specified serializer type. |

### JSON Serializer

| Method | Return Type | Parameters | Description |
|---|---|---|---|
| `SerializeWithJson<TIn, TOut>()` | `IProducePipelineStepBuilder<TOut, TOut>` | — | Serialize message to JSON (UTF-8 bytes). |
| `SerializeKeyWithJson<TIn, TOut, TKey>()` | `IProducePipelineStepBuilder<TOut, TOut>` | `Func<TOut, TKey> keyProvider` | Serialize message key to JSON (UTF-8 bytes). |

### Protobuf Serializer

| Method | Return Type | Parameters | Constraints | Description |
|---|---|---|---|---|
| `SerializeWithProtobuf<TIn, TOut>()` | `IProducePipelineStepBuilder<TOut, TOut>` | — | `TOut : IMessage` | Serialize message to protobuf bytes. |
| `SerializeKeyWithProtobuf<TIn, TOut, TKey>()` | `IProducePipelineStepBuilder<TOut, TOut>` | `Func<TOut, TKey> keyProvider` | `TKey : IMessage` | Serialize message key to protobuf bytes. |

### Example

```csharp
pipeline
    // JSON
    .SerializeWithJson()

    // Protobuf (requires Google.Protobuf)
    .SerializeWithProtobuf()

    // Custom serializer
    .Serialize<MyMessagePackSerializer, MyModel, byte[]>()

    // Key serialization
    .SerializeKeyWithJson(message => message.Id)
    .SerializeWithProtobuf();
```

## Pipeline Step Builder

### IPipelineStepBuilder&lt;TContext, TIn&gt;

The base interface for all step builders.

| Method | Description |
|---|---|
| `IPipelineStep<TContext, TIn> Build()` | Builds the pipeline step. Called internally during pipeline construction. |