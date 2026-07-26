using Microsoft.Extensions.Logging;

namespace OutboxFlow.Consume;

partial class OutboxConsumer
{
    private static partial class Log
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Fetched {Count} messages.")]
        public static partial void FetchedMessages(ILogger logger, int count);

        [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Delivered {Count} messages.")]
        public static partial void DeliveredMessages(ILogger logger, int count);

        [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Deleted {Count} messages.")]
        public static partial void DeletedMessages(ILogger logger, int count);

        [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Pipeline execution failed. Lock released.")]
        public static partial void PipelineExecutionFailed(ILogger logger, Exception exception);
    }
}