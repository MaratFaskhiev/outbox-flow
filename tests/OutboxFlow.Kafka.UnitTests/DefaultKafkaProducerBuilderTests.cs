using Confluent.Kafka;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace OutboxFlow.Kafka.UnitTests;

public sealed class DefaultKafkaProducerBuilderTests : IDisposable
{
    private readonly DefaultKafkaProducerBuilder _builder;
    private readonly Mock<IKafkaProducerRegistry> _registry = new(MockBehavior.Strict);

    public DefaultKafkaProducerBuilderTests()
    {
        _builder = new DefaultKafkaProducerBuilder(_registry.Object, NullLogger<DefaultKafkaProducerBuilder>.Instance);
    }

    public void Dispose()
    {
        Mock.VerifyAll(_registry);
    }

    [Fact]
    public void Create_ReturnsProducer()
    {
        var producer = _builder.Create(new ProducerConfig());

        Assert.NotNull(producer);
    }

    [Fact]
    public void OnError_Fatal_RemovesProducer()
    {
        var producerConfig = new ProducerConfig();
        var producer = _builder.Create(producerConfig);

        _registry.Setup(x => x.Remove(producerConfig));

        _builder.OnError(producer, producerConfig, new Error(ErrorCode.Unknown, "error", true));
    }

    [Fact]
    public void OnError_NonFatal_DoesntRemoveProducer()
    {
        var producerConfig = new ProducerConfig();
        var producer = _builder.Create(producerConfig);

        _builder.OnError(producer, producerConfig, new Error(ErrorCode.Unknown, "error", false));

        _registry.Verify(x => x.Remove(producerConfig), Times.Never);
    }
}