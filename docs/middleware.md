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

- `Destination` â€” the message topic/queue name
- `Transaction` â€” the current database transaction (`IDbTransaction`)
- `ServiceProvider` â€” the DI service provider
- `CancellationToken` â€” cancellation token for the operation
- `Headers` â€” message headers (`IDictionary<string, string>`)
- `Key` â€” message key (`byte[]?`)
- `Value` â€” message value (`byte[]?`)

### Consume Context

`IConsumeContext` provides:

- `ServiceProvider` â€” the DI service provider
- `CancellationToken` â€” cancellation token for the operation

## Sync Middleware Example

See `samples/OutboxFlow.Sample/LoggingMiddleware.cs`:

<!-- SNIPPET: docs_mw_sync -->
internal sealed class LoggingMiddleware : IProduceSyncMiddleware<SampleTextModel, SampleTextModel>
{
    private static readonly Action<ILogger, string, Exception?> LogMessage =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0),
            "Produced message: {Value}");

    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public SampleTextModel Run(SampleTextModel message, IProduceContext context)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(context);

        LogMessage(_logger, message.Value, null);

        return message;
    }
}
<!-- ENDSNIPPET: docs_mw_sync -->

## Async Middleware Example

See `samples/OutboxFlow.Sample/AsyncLoggingMiddleware.cs`:

<!-- SNIPPET: docs_mw_async -->
internal sealed class AsyncLoggingMiddleware : IProduceAsyncMiddleware<SampleTextModel, SampleTextModel>
{
    private static readonly Action<ILogger, string, Exception?> LogMessage =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0),
            "Async produce middleware: {Value}");

    private readonly ILogger<AsyncLoggingMiddleware> _logger;

    public AsyncLoggingMiddleware(ILogger<AsyncLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async ValueTask<SampleTextModel> RunAsync(SampleTextModel message, IProduceContext context)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(context);

        await Task.Yield();

        LogMessage(_logger, message.Value, null);

        return message;
    }
}
<!-- ENDSNIPPET: docs_mw_async -->

## Consume Middleware Example

See `samples/OutboxFlow.Sample/ConsumeLoggingMiddleware.cs`:

<!-- SNIPPET: docs_mw_consume -->
internal sealed class ConsumeLoggingMiddleware : IConsumeSyncMiddleware<IOutboxMessage, IOutboxMessage>
{
    private static readonly Action<ILogger, string, Exception?> LogMessage =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0),
            "Consumed message for destination: {Destination}");

    private readonly ILogger<ConsumeLoggingMiddleware> _logger;

    public ConsumeLoggingMiddleware(ILogger<ConsumeLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public IOutboxMessage Run(IOutboxMessage message, IConsumeContext context)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(context);

        LogMessage(_logger, message.Destination!, null);

        return message;
    }
}
<!-- ENDSNIPPET: docs_mw_consume -->

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
| `AddSyncStep<TMiddleware, T>()` | Middleware preserves the type (`T` â†’ `T`). |
| `AddSyncStep<TMiddleware, TIn, TOut>()` | Middleware transforms `TIn` to `TOut`, pipeline continues as `TOut`. |
| `AddSyncStep<TMiddleware, TIn, TOut, TNext>()` | Middleware transforms `TIn` to `TOut`, pipeline continues as `TOut` â†’ `TNext`. |

The same variants exist for `AddAsyncStep` and for consume pipeline methods.

### DI Registration

Middleware classes using DI must be registered in the service collection:

<!-- SNIPPET: docs_mw_register -->
services.AddScoped<LoggingMiddleware>();
<!-- ENDSNIPPET: docs_mw_register -->

See `samples/OutboxFlow.Sample/Program.cs` for the complete registration example.
