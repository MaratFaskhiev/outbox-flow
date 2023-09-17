using System.Text;
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
    /// <typeparam name="TKafkaProducerBuilder">Producer builder type.</typeparam>
    public static IConsumePipelineStepBuilder<IOutboxMessage, IOutboxMessage> SendToKafka<TIn, TKafkaProducerBuilder>(
        this IConsumePipelineStepBuilder<TIn, IOutboxMessage> pipeline,
        ProducerConfig producerConfig)
        where TKafkaProducerBuilder : IKafkaProducerBuilder
    {
        return pipeline.AddAsyncStep(async (message, context) =>
        {
            var producerRegistry = context.ServiceProvider.GetRequiredService<IKafkaProducerRegistry>();
            var producerBuilder = context.ServiceProvider.GetRequiredService<TKafkaProducerBuilder>();
            var producer = producerRegistry.GetOrCreate(producerBuilder, producerConfig);

            var kafkaMessage = new Message<byte[], byte[]>();
            if (message.Headers.Any()) kafkaMessage.Headers = new Headers();
            foreach (var header in message.Headers)
                kafkaMessage.Headers.Add(header.Key, Encoding.UTF8.GetBytes(header.Value));
            if (message.Key != null)
                kafkaMessage.Key = message.Key;
            kafkaMessage.Value = message.Value;
            try
            {
                await producer.ProduceAsync(message.Destination, kafkaMessage, context.CancellationToken)
                    .ConfigureAwait(false);
            }
            catch (ProduceException<byte[], byte[]> exception)
            {
                if (exception.Error.IsFatal)
                {
                    producerRegistry.Remove(producerConfig);
                    producer.Dispose();
                }

                throw;
            }

            return message;
        });
    }

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
        return pipeline.SendToKafka<TIn, DefaultKafkaProducerBuilder>(producerConfig);
    }
}