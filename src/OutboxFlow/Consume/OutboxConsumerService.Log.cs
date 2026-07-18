using Microsoft.Extensions.Logging;

namespace OutboxFlow.Consume;

partial class OutboxConsumerService
{
    private static partial class Log
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Service is started.")]
        public static partial void ServiceStarted(ILogger logger);

        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to process outbox messages.")]
        public static partial void FailedToProcess(ILogger logger, Exception exception);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Service is stopped.")]
        public static partial void ServiceStopped(ILogger logger);
    }
}