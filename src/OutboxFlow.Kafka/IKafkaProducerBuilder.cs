using Confluent.Kafka;

namespace OutboxFlow.Kafka;

/// <summary>
/// Kafka producer builder.
/// </summary>
public interface IKafkaProducerBuilder
{
    /// <summary>
    /// Creates the Kafka producer for the specified configuration.
    /// </summary>
    /// <param name="producerConfig">Producer configuration.</param>
    IProducer<byte[], byte[]> Create(ProducerConfig producerConfig);
}