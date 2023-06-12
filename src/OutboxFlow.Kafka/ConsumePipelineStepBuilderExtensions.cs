using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using OutboxFlow.Configuration;

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
    /// <typeparam name="TOut">Current step output message type.</typeparam>
    public static IConsumePipelineStepBuilder<TOut, TOut> SendToKafka<TIn, TOut>(
        this IConsumePipelineStepBuilder<TIn, TOut> pipeline,
        ProducerConfig producerConfig)
    {
        return pipeline.AddStep(async (message, context) =>
        {
            var producerRegistry = context.ServiceProvider.GetRequiredService<IKafkaProducerRegistry>();
            var producer = producerRegistry.GetOrCreate(producerConfig);

            var kafkaMessage = new Message<byte[], byte[]>();
            if (context.Key != null)
                kafkaMessage.Key = context.Key;
            kafkaMessage.Value = context.Value;
            await producer.ProduceAsync(context.Destination, kafkaMessage, context.CancellationToken)
                .ConfigureAwait(false);

            return message;
        });
    }
}