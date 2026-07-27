using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OutboxFlow.Kafka;

partial class DefaultKafkaProducerBuilder
{
    private static partial class Log
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "Kafka producer {Name} error: {Code} ({Reason})")]
        public static partial void FatalError(ILogger logger, string name, ErrorCode code, string reason);

        [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Kafka producer {Name} error: {Code} ({Reason})")]
        public static partial void NonFatalError(ILogger logger, string name, ErrorCode code, string reason);
    }
}