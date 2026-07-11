using Microsoft.Extensions.Logging;
using OutboxFlow.Produce;
using OutboxFlow.Sample.Models;

namespace OutboxFlow.Sample;

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