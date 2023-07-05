using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using OutboxFlow.Consume.Configuration;
using OutboxFlow.Storage;

namespace OutboxFlow.Kafka;

/// <summary>
/// Extension methods for setting up Apache Kafka as a destination.
/// </summary>
public static class ConsumePipelineStepBuilderExtensions
{
    /// <summary>
    /// Sends the message to the Kafka topic.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <param name="producerConfig">Producer config.</param>
    /// <typeparam name="TIn">Current step input message type.</typeparam>
    public static IConsumePipelineStepBuilder<IOutboxMessage, IOutboxMessage> SendToKafka<TIn>(
        this IConsumePipelineStepBuilder<TIn, IOutboxMessage> pipeline,
        ProducerConfig producerConfig)
    {
        return pipeline.AddAsyncStep(async (message, context) =>
        {
            var producerRegistry = context.ServiceProvider.GetRequiredService<IKafkaProducerRegistry>();
            var producer = producerRegistry.GetOrCreate(producerConfig);

            var kafkaMessage = new Message<byte[], byte[]>();
            if (message.Key != null)
                kafkaMessage.Key = message.Key;
            kafkaMessage.Value = message.Value;
            await producer.ProduceAsync(message.Destination, kafkaMessage, context.CancellationToken)
                .ConfigureAwait(false);

            return message;
        });
    }
}