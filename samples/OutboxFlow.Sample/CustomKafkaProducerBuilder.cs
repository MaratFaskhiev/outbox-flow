using Confluent.Kafka;
using OutboxFlow.Kafka;

namespace OutboxFlow.Sample;

public sealed class CustomKafkaProducerBuilder : IKafkaProducerBuilder
{
    public IProducer<byte[], byte[]> Create(ProducerConfig producerConfig)
    {
        return new ProducerBuilder<byte[], byte[]>(producerConfig)
            .Build();
    }
}