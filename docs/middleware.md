# Middleware Development Guide

OutboxFlow supports custom middleware for both produce and consume pipelines. Middleware allows you to intercept and transform messages during pipeline execution.

## Middleware Interfaces

There are four middleware interfaces, covering sync/async for produce and consume pipelines:

| Interface | Method | Description |
|---|---|---|
| `IProduceSyncMiddleware<TIn, TOut>` | `TOut Run(TIn message, IProduceContext context)` | Synchronous produce middleware. |
| `IProduceAsyncMiddleware<TIn, TOut>` | `ValueTask<TOut> RunAsync(TIn message, IProduceContext context)` | Asynchronous produce middleware. |
| `IConsumeSyncMiddleware<TIn, TOut>` | `TOut Run(TIn message, IConsumeContext context)` | Synchronous consume middleware. |
| `IConsumeAsyncMiddleware<TIn, TOut>` | `ValueTask<TOut> RunAsync(TIn message, IConsumeContext context)` | Asynchronous consume middleware. |

All middleware interfaces derive from the base `ISyncMiddleware<TContext, TIn, TOut>` or `IAsyncMiddleware<TContext, TIn, TOut>`.

### Produce Context

`IProduceContext` provides access to:

- `Destination` — the message topic/queue name
- `Transaction` — the current database transaction (`IDbTransaction`)
- `ServiceProvider` — the DI service provider
- `CancellationToken` — cancellation token for the operation
- `Headers` — message headers (`IDictionary<string, string>`)
- `Key` — message key (`byte[]?`)
- `Value` — message value (`byte[]?`)

### Consume Context

`IConsumeContext` provides:

- `ServiceProvider` — the DI service provider
- `CancellationToken` — cancellation token for the operation

## Sync Middleware Example

See `samples/OutboxFlow.Sample/LoggingMiddleware.cs`:

```csharp
public sealed class LoggingMiddleware : IProduceSyncMiddleware<SampleTextModel, SampleTextModel>
{
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public SampleTextModel Run(SampleTextModel message, IProduceContext context)
    {
        _logger.LogInformation("Produced message: {Value}", message.Value);
        return message;
    }
}
```

## Async Middleware Example

See `samples/OutboxFlow.Sample/AsyncLoggingMiddleware.cs`:

```csharp
public sealed class AsyncLoggingMiddleware : IProduceAsyncMiddleware<SampleTextModel, SampleTextModel>
{
    private readonly ILogger<AsyncLoggingMiddleware> _logger;

    public AsyncLoggingMiddleware(ILogger<AsyncLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async ValueTask<SampleTextModel> RunAsync(SampleTextModel message, IProduceContext context)
    {
        await Task.Yield();
        _logger.LogInformation("Async produce middleware: {Value}", message.Value);
        return message;
    }
}
```

## Consume Middleware Example

See `samples/OutboxFlow.Sample/ConsumeLoggingMiddleware.cs`:

```csharp
public sealed class ConsumeLoggingMiddleware : IConsumeSyncMiddleware<IOutboxMessage, IOutboxMessage>
{
    private readonly ILogger<ConsumeLoggingMiddleware> _logger;

    public ConsumeLoggingMiddleware(ILogger<ConsumeLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public IOutboxMessage Run(IOutboxMessage message, IConsumeContext context)
    {
        _logger.LogInformation("Consumed message for destination: {Destination}", message.Destination);
        return message;
    }
}
```

## Middleware Registration

Middleware is registered via the `AddSyncStep` or `AddAsyncStep` extension methods on the pipeline builder.

### Produce Pipeline

```csharp
producer.ForMessage<SampleTextModel>(pipeline =>
    pipeline
        .AddSyncStep<LoggingMiddleware, SampleTextModel>()
        .AddAsyncStep<AsyncLoggingMiddleware, SampleTextModel>()
        .SerializeWithJson()
        .SetDestination("topic")
        .Save()
);
```

### Consume Pipeline

```csharp
consumer.SetDefaultRoute(pipeline =>
    pipeline
        .AddSyncStep<ConsumeLoggingMiddleware, IOutboxMessage, IOutboxMessage>()
        .SendToKafka(producerConfig)
);
```

### Type Parameter Variants

The middleware generic methods support scenarios where the middleware transforms the message type:

| Method | Behavior |
|---|---|
| `AddSyncStep<TMiddleware, T>()` | Middleware preserves the type (`T` → `T`). |
| `AddSyncStep<TMiddleware, TIn, TOut>()` | Middleware transforms `TIn` to `TOut`, pipeline continues as `TOut`. |
| `AddSyncStep<TMiddleware, TIn, TOut, TNext>()` | Middleware transforms `TIn` to `TOut`, pipeline continues as `TOut` → `TNext`. |

The same variants exist for `AddAsyncStep` and for consume pipeline methods.

### DI Registration

Middleware classes using DI must be registered in the service collection:

```csharp
services.AddScoped<LoggingMiddleware>();
services.AddScoped<AsyncLoggingMiddleware>();
services.AddScoped<ConsumeLoggingMiddleware>();
```

See `samples/OutboxFlow.Sample/Program.cs` for the complete registration example.