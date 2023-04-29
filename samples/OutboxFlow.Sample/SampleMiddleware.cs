using Microsoft.Extensions.Logging;
using OutboxFlow.Abstractions;

namespace OutboxFlow.Sample;

public sealed class SampleMiddleware<T> : IProduceMiddleware<T, T>
{
    private readonly ILogger<SampleMiddleware<T>> _logger;

    private int _counter;

    public SampleMiddleware(ILogger<SampleMiddleware<T>> logger)
    {
        _logger = logger;
    }

    public ValueTask<T> InvokeAsync(T message, IProduceContext context)
    {
        _counter++;
        _logger.LogInformation($"Middleware invoked: {_counter}");

        return new ValueTask<T>(message);
    }
}