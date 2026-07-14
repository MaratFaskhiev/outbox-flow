# Architecture

OutboxFlow implements the [transactional outbox pattern](https://microservices.io/patterns/data/transactional-outbox.html) using a linked-list pipeline. Messages are saved to a database within the same transaction as the business operation, then asynchronously delivered to a message broker by a background service.

## Pipeline Pattern

The core execution model is a chain of `PipelineStep<TContext, TIn, TOut>` instances, each wrapping a function (sync or async) and an optional reference to the next step.

```
message → [Step 1] → [Step 2] → [Step 3] → ... → [Step N]
         Run      Run      Run                Run
```

Each step receives a message of type `TIn`, transforms or inspects it, and passes a result of type `TOut` to the next step. The chain is built using fluent builder methods.

### PipelineStep

A step holds either a synchronous function (`Func<TIn, TContext, TOut>`) or an asynchronous function (`Func<TIn, TContext, ValueTask<TOut>>`), plus a reference to the next step. When `RunAsync` is called, the step executes its function and forwards the result to the next step.

```csharp
public sealed class PipelineStep<TContext, TIn, TOut> : IPipelineStep<TContext, TIn>
{
    // Async constructor
    public PipelineStep(
        Func<TIn, TContext, ValueTask<TOut>> action,
        IPipelineStep<TContext, TOut>? nextStep);

    // Sync constructor
    public PipelineStep(
        Func<TIn, TContext, TOut> action,
        IPipelineStep<TContext, TOut>? nextStep);
}
```

### Pipeline

A wrapper that holds the first step (or null for an empty pipeline) and delegates to it.

```csharp
public sealed class Pipeline<TContext, T> : IPipelineStep<TContext, T>
{
    public Pipeline(IPipelineStep<TContext, T>? step);
}
```

### IPipelineStep

All steps and pipelines implement the same fundamental interface:

```csharp
public interface IPipelineStep<TContext, T>
{
    ValueTask RunAsync(T message, TContext context);
}
```

## Contexts

Two context types carry state through the pipeline:

### IProduceContext

Used on the produce side. Carries the database transaction, service provider for DI, cancellation token, and mutable message properties.

```csharp
public interface IProduceContext
{
    string? Destination { get; set; }
    IDbTransaction Transaction { get; }
    IServiceProvider ServiceProvider { get; }
    CancellationToken CancellationToken { get; }
    IDictionary<string, string> Headers { get; }
    byte[]? Key { get; set; }
    byte[]? Value { get; set; }
}
```

### IConsumeContext

Used on the consume side. Simpler — provides service provider and cancellation token.

```csharp
public interface IConsumeContext
{
    IServiceProvider ServiceProvider { get; }
    CancellationToken CancellationToken { get; }
}
```

## Message Lifecycle

### 1. Produce

`IProducer.ProduceAsync<T>(T message, IDbTransaction transaction, CancellationToken)` is called by application code within a database transaction.

```
Application Code
     │
     ▼
Producer
     │  Gets pipeline from IProducePipelineRegistry
     │  Creates ProduceContext
     ▼
Pipeline chain:
  [AddSyncStep: transform/log the message]
  [Serialize: convert to byte[] via ISerializer]
  [SetDestination: assign topic/queue name]
  [SetKey: assign message key]
  [AddHeader: attach metadata]
  [Save: IOutboxStorage.SaveAsync(context)]
     │
     ▼
  outbox_message table (within the same DB transaction)
```

### 2. Consume

`OutboxConsumerService` is a `BackgroundService` that runs continuously. In each iteration:

```
OutboxConsumerService (BackgroundService)
     │
     ▼ loop:
  OutboxConsumer.ConsumeAsync()
     │
     ├─ Open DB connection
     ├─ Begin transaction (lock + fetch)
     ├─ IOutboxLockManager.LockAsync() — acquire lock on outbox_state
     ├─ IOutboxStorage.FetchAsync() — read batch of messages
     ├─ Commit transaction (release lock, keep messages)
     │
     ▼ for each message:
  IConsumePipelineRegistry.GetPipeline(destination)
     │
     ▼ Consume pipeline:
  [SendToKafka → Confluent.Kafka IProducer]
     │
     ├─ Begin transaction (delete + release)
     ├─ IOutboxStorage.DeleteAsync() — remove processed messages
     ├─ IOutboxLockManager.ReleaseAsync() — release the lock
     └─ Commit transaction
```

If `ConsumeAsync` fails (lock not acquired, error in pipeline), the service waits and retries.

### 3. Batch Produce

Batch produce uses `Producer.ProduceAsync<IReadOnlyCollection<T>>(...)` with a pipeline configured with `ForEach` and `SaveBatch`:

```
Application Code
     │
     ▼
Producer.ProduceAsync<IReadOnlyCollection<T>>
     │  Gets pipeline from IProducePipelineRegistry
     │  ForEach step — for each message:
     │     Creates ProduceContext (inherits outer headers)
     │     Runs sub-pipeline:
     │       [Serialize] [SetDestination] [SetKey] ...
     ▼
     All messages processed
     │
     ▼
   SaveBatch — single storage operation (e.g., NpgsqlBatch)
     │
     ▼
   outbox_message table (within the same DB transaction)
```

Each message runs through the full pipeline individually, but all messages are persisted in a single storage operation. If any pipeline step throws, the exception propagates and the caller's transaction handles rollback — no partial batch is saved.

## Extension Points

| Interface | Purpose | Implement to... |
|---|---|---|
| `IOutboxStorage` | Persist and retrieve outbox messages | Add a new storage provider (e.g., SQL Server, MongoDB) |
| `IOutboxLockManager` | Coordinate consumer access | Change locking strategy (e.g., Redis lock, lease-based) |
| `IDbConnectionFactory` | Create ADO.NET connections | Control connection creation |
| `IProduceSyncMiddleware<TIn, TOut>` | Synchronous produce step | Add logging, validation, transformation |
| `IProduceAsyncMiddleware<TIn, TOut>` | Asynchronous produce step | Call external services during produce |
| `IConsumeSyncMiddleware<TIn, TOut>` | Synchronous consume step | Transform or inspect consumed messages |
| `IConsumeAsyncMiddleware<TIn, TOut>` | Asynchronous consume step | Call external services during consume |
| `ISerializer<T>` | Serialize message body/Key | Add MessagePack, XML, or custom serialization |
| `IKafkaProducerBuilder` | Create Kafka producer instances | Customize Kafka producer configuration |

## Outbox Storage Schema (PostgreSQL)

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

- `outbox_message` stores produced messages before delivery
- `outbox_state` coordinates multiple consumer instances (only one consumer reads at a time)

## Related Documents

- [Getting Started](getting-started.md) — step-by-step setup
- [Middleware](middleware.md) — creating custom pipeline steps
- [Storage](storage.md) — implementing storage providers
- [Configuration](configuration.md) — full API reference