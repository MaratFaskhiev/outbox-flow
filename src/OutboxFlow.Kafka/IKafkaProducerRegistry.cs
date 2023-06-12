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
    /// <param name="producerConfig">Producer configuration.</param>
    IProducer<byte[], byte[]> GetOrCreate(ProducerConfig producerConfig);
}