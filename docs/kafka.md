# Kafka Destination Guide

OutboxFlow uses Apache Kafka as the default destination for consumed outbox messages. The `SendToKafka` extension method sends messages to a Kafka topic.

## Service Registration

Before configuring the outbox, register Kafka dependencies:

```csharp
services.AddKafka();
```

This registers:
- `DefaultKafkaProducerBuilder` — the default Kafka producer builder
- `IKafkaProducerBuilder` — registered via factory that returns the same `DefaultKafkaProducerBuilder` instance
- `IKafkaProducerRegistry` — manages producer instances and reuses them across messages

### Recommended Configuration

For reliable outbox delivery with exactly-once semantics, use `WithOutboxDefaults()`:

```csharp
var producerConfig = new ProducerConfig
{
    BootstrapServers = "localhost:9092"
}.WithOutboxDefaults();
```

This sets `EnableIdempotence=true` and `Acks=All` unless explicitly overridden.

> **Note:** `ProducerConfig` is compared by reference equality in the producer registry.
> Always reuse the same config instance for all messages sharing the same broker settings.

## SendToKafka

The consume pipeline uses `SendToKafka` to forward messages to Kafka:

```csharp
consumer.SetDefaultRoute(pipeline =>
    pipeline.SendToKafka(producerConfig)
);
```

There are two overloads:

| Method | Description |
|---|---|
| `SendToKafka<TIn>(ProducerConfig producerConfig)` | Sends the message using `DefaultKafkaProducerBuilder`. |
| `SendToKafka<TIn, TKafkaProducerBuilder>(ProducerConfig producerConfig)` | Sends the message using a custom `IKafkaProducerBuilder`. |

The method reads `IOutboxMessage` properties (`Destination`, `Key`, `Value`, `Headers`) and produces a `Message<byte[], byte[]>` to the specified Kafka topic.

### Route-Specific Configuration

You can configure different pipelines for different destinations:

```csharp
consumer
    .SetDefaultRoute(pipeline => pipeline.SendToKafka(defaultConfig))
    .AddRoute("priority-topic", pipeline => pipeline.SendToKafka(priorityConfig));
```

## IKafkaProducerBuilder

Implement this interface to customize how Kafka producers are created:

```csharp
public interface IKafkaProducerBuilder
{
    IProducer<byte[], byte[]> Create(ProducerConfig producerConfig);
}
```

### DefaultKafkaProducerBuilder

The default implementation creates a standard `ProducerBuilder<byte[], byte[]>` with error handling that logs errors and removes dead producers from the registry on fatal errors.

## Custom IKafkaProducerBuilder

See `samples/OutboxFlow.Sample/CustomKafkaProducerBuilder.cs` for a complete example:

```csharp
public sealed class CustomKafkaProducerBuilder : IKafkaProducerBuilder
{
    public IProducer<byte[], byte[]> Create(ProducerConfig producerConfig)
    {
        return new ProducerBuilder<byte[], byte[]>(producerConfig)
            .Build();
    }
}
```

Register the custom builder and use it:

```csharp
services.AddSingleton<IKafkaProducerBuilder, CustomKafkaProducerBuilder>();

// In consumer configuration:
consumer.SetDefaultRoute(pipeline =>
    pipeline.SendToKafka<IOutboxMessage, CustomKafkaProducerBuilder>(producerConfig)
);
```

## IKafkaProducerRegistry

Manages the lifecycle of Kafka producer instances.

| Method | Description |
|---|---|
| `GetOrCreate(IKafkaProducerBuilder, ProducerConfig)` | Returns an existing producer for the given config, or creates a new one. |
| `Remove(ProducerConfig)` | Removes and disposes a producer (called on fatal errors). |

The registry ensures that producers are reused across messages, and that dead producers (after fatal errors) are replaced.