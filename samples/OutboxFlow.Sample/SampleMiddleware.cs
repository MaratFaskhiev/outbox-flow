using Microsoft.Extensions.Logging;
using OutboxFlow.Abstractions;

namespace OutboxFlow.Sample;

public sealed class SampleMiddleware<T> : IProduceSyncMiddleware<T, T>
{
    private readonly ILogger<SampleMiddleware<T>> _logger;

    private int _counter;

    public SampleMiddleware(ILogger<SampleMiddleware<T>> logger)
    {
        _logger = logger;
    }

    public T Invoke(T message, IProduceContext context)
    {
        _counter++;
        _logger.LogInformation("Middleware invoked: {Counter}", _counter);

        return message;
    }
}