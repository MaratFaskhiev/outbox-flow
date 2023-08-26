using Confluent.Kafka;
using Moq;
using Xunit;

namespace OutboxFlow.Kafka.UnitTests;

public sealed class KafkaProducerRegistryTests : IDisposable
{
    private readonly Mock<IKafkaProducerBuilder> _builder = new(MockBehavior.Strict);
    private readonly KafkaProducerRegistry _registry = new();

    public void Dispose()
    {
        Mock.VerifyAll(_builder);

        _registry.Dispose();
    }

    [Fact]
    public void GetOrCreate_SameConfig_ReturnsSameProducer()
    {
        var producerConfig = new ProducerConfig();

        _builder.Setup(x => x.Create(producerConfig)).Returns(Mock.Of<IProducer<byte[], byte[]>>);

        var producer1 = _registry.GetOrCreate(_builder.Object, producerConfig);
        var producer2 = _registry.GetOrCreate(_builder.Object, producerConfig);

        Assert.Same(producer1, producer2);

        _builder.Verify(x => x.Create(producerConfig), Times.Once);
    }

    [Fact]
    public void Dispose_DisposesProducers()
    {
        var producerConfig = new ProducerConfig();

        var producer = new Mock<IProducer<byte[], byte[]>>(MockBehavior.Strict);
        producer.Setup(x => x.Flush(CancellationToken.None));
        producer.Setup(x => x.Dispose());
        _builder.Setup(x => x.Create(producerConfig)).Returns(producer.Object);

        _registry.GetOrCreate(_builder.Object, producerConfig);
        _registry.Dispose();

        producer.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public void Remove_ProducerIsFound_RemovesProducer()
    {
        var producerConfig = new ProducerConfig();

        var producer = new Mock<IProducer<byte[], byte[]>>();
        _builder.Setup(x => x.Create(producerConfig)).Returns(producer.Object);

        _registry.GetOrCreate(_builder.Object, producerConfig);
        _registry.Remove(producerConfig);

        var newProducer = _registry.GetOrCreate(_builder.Object, producerConfig);

        Assert.NotSame(producer, newProducer);
    }

    [Fact]
    public void Invalidate_ProducerIsNotFound_DoesNothing()
    {
        var producerConfig1 = new ProducerConfig();
        var producerConfig2 = new ProducerConfig();

        var producer = new Mock<IProducer<byte[], byte[]>>();
        _builder.Setup(x => x.Create(producerConfig1)).Returns(producer.Object);

        _registry.GetOrCreate(_builder.Object, producerConfig1);
        _registry.Remove(producerConfig2);
    }
}