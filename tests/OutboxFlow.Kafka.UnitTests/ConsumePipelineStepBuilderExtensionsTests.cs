using Confluent.Kafka;
using Moq;
using OutboxFlow.Consume;
using OutboxFlow.Consume.Configuration;
using OutboxFlow.Storage;
using Xunit;

namespace OutboxFlow.Kafka.UnitTests;

public sealed class ConsumePipelineStepBuilderExtensionsTests : IDisposable
{
    private readonly Mock<IConsumePipelineStepBuilder<string, IOutboxMessage>> _builder;
    private readonly Mock<IConsumeContext> _context;
    private readonly Mock<IKafkaProducerRegistry> _kafkaProducerRegistry;
    private readonly Mock<IConsumePipelineStepBuilder<IOutboxMessage, IOutboxMessage>> _nextBuilder;
    private readonly Mock<IOutboxMessage> _outboxMessage;
    private readonly Mock<IProducer<byte[], byte[]>> _producer;
    private readonly Mock<IServiceProvider> _serviceProvider;

    public ConsumePipelineStepBuilderExtensionsTests()
    {
        _builder = new Mock<IConsumePipelineStepBuilder<string, IOutboxMessage>>(MockBehavior.Strict);
        _nextBuilder = new Mock<IConsumePipelineStepBuilder<IOutboxMessage, IOutboxMessage>>(MockBehavior.Strict);
        _outboxMessage = new Mock<IOutboxMessage>(MockBehavior.Strict);
        _context = new Mock<IConsumeContext>(MockBehavior.Strict);
        _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
        _kafkaProducerRegistry = new Mock<IKafkaProducerRegistry>(MockBehavior.Strict);
        _producer = new Mock<IProducer<byte[], byte[]>>(MockBehavior.Strict);

        _context.Setup(x => x.ServiceProvider).Returns(_serviceProvider.Object);
        _serviceProvider.Setup(x => x.GetService(typeof(IKafkaProducerRegistry)))
            .Returns(_kafkaProducerRegistry.Object);
    }

    public void Dispose()
    {
        Mock.VerifyAll(
            _builder,
            _nextBuilder,
            _outboxMessage,
            _context,
            _serviceProvider,
            _kafkaProducerRegistry,
            _producer);
    }

    [Fact]
    public async Task SendToKafka_AddsProduceStep()
    {
        var producerConfig = new ProducerConfig();

        Func<IOutboxMessage, IConsumeContext, ValueTask<IOutboxMessage>>? action = null;
        _builder
            .Setup(x => x.AddAsyncStep(It.IsAny<Func<IOutboxMessage, IConsumeContext, ValueTask<IOutboxMessage>>>()))
            .Returns(_nextBuilder.Object)
            .Callback((Func<IOutboxMessage, IConsumeContext, ValueTask<IOutboxMessage>> a) => { action = a; });

        _builder.Object.SendToKafka(producerConfig);

        Assert.NotNull(action);

        _kafkaProducerRegistry.Setup(x => x.GetOrCreate(producerConfig))
            .Returns(_producer.Object);

        var key = Guid.NewGuid().ToByteArray();
        _outboxMessage.Setup(x => x.Key).Returns(key);

        var value = Guid.NewGuid().ToByteArray();
        _outboxMessage.Setup(x => x.Value).Returns(value);

        var destination = "destination";
        _outboxMessage.Setup(x => x.Destination).Returns(destination);

        var cancellationToken = new CancellationToken();
        _context.Setup(x => x.CancellationToken).Returns(cancellationToken);

        _producer.Setup(x => x.ProduceAsync(destination,
                It.Is<Message<byte[], byte[]>>(m => m.Key == key && m.Value == value), cancellationToken))
            .ReturnsAsync(new DeliveryReport<byte[], byte[]>());

        await action(_outboxMessage.Object, _context.Object);

        _producer.Verify(
            x => x.ProduceAsync(
                destination,
                It.Is<Message<byte[], byte[]>>(m => m.Key == key && m.Value == value),
                cancellationToken),
            Times.Once);
    }
}