using FluentAssertions;
using Moq;
using OutboxFlow.Produce;
using Xunit;

namespace OutboxFlow.UnitTests.Produce;

public sealed class ProducerTests : IDisposable
{
    private readonly Mock<IPipelineStep<IProduceContext, string>> _pipelineStep;

    private readonly Producer _producer;
    private readonly Mock<IProducePipelineRegistry> _registry;
    private readonly Mock<IServiceProvider> _serviceProvider;

    public ProducerTests()
    {
        _registry = new Mock<IProducePipelineRegistry>(MockBehavior.Strict);
        _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
        _pipelineStep = new Mock<IPipelineStep<IProduceContext, string>>(MockBehavior.Strict);

        _producer = new Producer(_registry.Object, _serviceProvider.Object);
    }

    public void Dispose()
    {
        Mock.VerifyAll(_registry, _serviceProvider, _pipelineStep);
    }

    [Fact]
    public async Task ProduceAsync_RunsPipeline()
    {
        var message = "test";

        _registry.Setup(x => x.GetPipeline<string>()).Returns(_pipelineStep.Object);

        IProduceContext? context = null;
        _pipelineStep.Setup(x => x.RunAsync(message, It.IsAny<IProduceContext>()))
            .Callback((string _, IProduceContext ctx) => { context = ctx; })
            .Returns(new ValueTask());

        await _producer.ProduceAsync(message, CancellationToken.None);

        context.Should().NotBeNull();
        context!.ServiceProvider.Should().BeSameAs(_serviceProvider.Object);
        context.CancellationToken.IsCancellationRequested.Should().BeFalse();
    }

    [Fact]
    public async Task ProduceAsync_CollectionType_CreatesContextAndRunsPipeline()
    {
        IReadOnlyCollection<string> messages = new[] {"a", "b"};

        var collectionPipelineStep =
            new Mock<IPipelineStep<IProduceContext, IReadOnlyCollection<string>>>(MockBehavior.Strict);

        _registry.Setup(x => x.GetPipeline<IReadOnlyCollection<string>>())
            .Returns(collectionPipelineStep.Object);

        IProduceContext? context = null;
        collectionPipelineStep.Setup(x => x.RunAsync(messages, It.IsAny<IProduceContext>()))
            .Callback((IReadOnlyCollection<string> _, IProduceContext ctx) => { context = ctx; })
            .Returns(new ValueTask());

        await _producer.ProduceAsync(messages, CancellationToken.None);

        context.Should().NotBeNull();
        context!.ServiceProvider.Should().BeSameAs(_serviceProvider.Object);
        context.CancellationToken.IsCancellationRequested.Should().BeFalse();

        collectionPipelineStep.VerifyAll();
    }
}