using Microsoft.Extensions.Logging;
using OutboxFlow.Consume;
using OutboxFlow.Storage;

namespace OutboxFlow.Sample;

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