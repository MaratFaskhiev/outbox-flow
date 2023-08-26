using Confluent.Kafka;

namespace OutboxFlow.Kafka;

/// <summary>
/// Kafka producer registry.
/// </summary>
public interface IKafkaProducerRegistry
{
    /// <summary>
    /// Gets or creates the Kafka producer for the specified configuration.
    /// </summary>
    /// <param name="producerBuilder">Producer builder.</param>
    /// <param name="producerConfig">Producer configuration.</param>
    IProducer<byte[], byte[]> GetOrCreate(IKafkaProducerBuilder producerBuilder, ProducerConfig producerConfig);

    /// <summary>
    /// Removes the Kafka producer for the specified configuration.
    /// </summary>
    /// <param name="producerConfig">Producer configuration.</param>
    void Remove(ProducerConfig producerConfig);
}