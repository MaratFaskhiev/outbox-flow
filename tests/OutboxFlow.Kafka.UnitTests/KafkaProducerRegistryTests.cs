using Confluent.Kafka;
using Moq;
using Xunit;

namespace OutboxFlow.Kafka.UnitTests;

public sealed class KafkaProducerRegistryTests : IDisposable
{
    private readonly Mock<IKafkaProducerBuilder> _builder;

#pragma warning disable CA2213
    private readonly KafkaProducerRegistry _registry;
#pragma warning restore CA2213

    public KafkaProducerRegistryTests()
    {
        _builder = new Mock<IKafkaProducerBuilder>(MockBehavior.Strict);

        _registry = new KafkaProducerRegistry(_builder.Object);
    }

    public void Dispose()
    {
        Mock.VerifyAll(_builder);
    }

    [Fact]
    public void GetOrCreate_SameConfig_ReturnsSameProducer()
    {
        var producerConfig = new ProducerConfig();

        _builder.Setup(x => x.Create(producerConfig)).Returns(Mock.Of<IProducer<byte[], byte[]>>);

        var producer1 = _registry.GetOrCreate(producerConfig);
        var producer2 = _registry.GetOrCreate(producerConfig);

        Assert.Same(producer1, producer2);

        _builder.Verify(x => x.Create(producerConfig), Times.Once);
    }

    [Fact]
    public void Dispose_DisposesProducers()
    {
        var producerConfig = new ProducerConfig();

        var producer = new Mock<IProducer<byte[], byte[]>>(MockBehavior.Strict);
        producer.Setup(x => x.Dispose());
        _builder.Setup(x => x.Create(producerConfig)).Returns(producer.Object);

        _registry.GetOrCreate(producerConfig);
        _registry.Dispose();

        producer.Verify(x => x.Dispose(), Times.Once);
    }
}