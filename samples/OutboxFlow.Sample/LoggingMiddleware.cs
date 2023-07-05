using Microsoft.Extensions.Logging;
using OutboxFlow.Produce;
using OutboxFlow.Sample.Models;

namespace OutboxFlow.Sample;

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