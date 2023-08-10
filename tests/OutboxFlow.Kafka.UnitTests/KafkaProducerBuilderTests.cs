using Confluent.Kafka;
using Xunit;

namespace OutboxFlow.Kafka.UnitTests;

public sealed class KafkaProducerBuilderTests
{
    private readonly KafkaProducerBuilder _builder = new();

    [Fact]
    public void Create_ReturnsProducer()
    {
        var producer = _builder.Create(new ProducerConfig());

        Assert.NotNull(producer);
    }
}