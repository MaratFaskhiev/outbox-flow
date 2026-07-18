using Microsoft.Extensions.Logging;
using OutboxFlow.Produce;
using OutboxFlow.Sample.Models;

namespace OutboxFlow.Sample;

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