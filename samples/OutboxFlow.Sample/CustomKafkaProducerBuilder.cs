using Confluent.Kafka;
using OutboxFlow.Kafka;

namespace OutboxFlow.Sample;

#region docs_ka_custom
internal sealed class CustomKafkaProducerBuilder : IKafkaProducerBuilder
{
    public IProducer<byte[], byte[]> Create(ProducerConfig producerConfig)
    {
        return new ProducerBuilder<byte[], byte[]>(producerConfig)
            .Build();
    }
}
#endregion