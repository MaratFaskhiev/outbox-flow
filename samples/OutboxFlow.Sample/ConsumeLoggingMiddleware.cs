using Microsoft.Extensions.Logging;
using OutboxFlow.Consume;
using OutboxFlow.Storage;

namespace OutboxFlow.Sample;

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