using Confluent.Kafka;

namespace OutboxFlow.Kafka;

/// <inheritdoc />
public sealed class KafkaProducerBuilder : IKafkaProducerBuilder
{
    /// <inheritdoc />
    public IProducer<byte[], byte[]> Create(ProducerConfig producerConfig)
    {
        return new ProducerBuilder<byte[], byte[]>(producerConfig).Build();
    }
}