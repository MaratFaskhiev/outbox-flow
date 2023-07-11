using System.Data;
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
    private readonly Mock<IDbTransaction> _transaction;

    public ProducerTests()
    {
        _registry = new Mock<IProducePipelineRegistry>(MockBehavior.Strict);
        _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
        _pipelineStep = new Mock<IPipelineStep<IProduceContext, string>>(MockBehavior.Strict);
        _transaction = new Mock<IDbTransaction>(MockBehavior.Strict);

        _producer = new Producer(_registry.Object, _serviceProvider.Object);
    }

    public void Dispose()
    {
        Mock.VerifyAll(_registry, _serviceProvider, _pipelineStep, _transaction);
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

        await _producer.ProduceAsync(message, _transaction.Object, new CancellationToken());

        Assert.NotNull(context);
        Assert.Same(_transaction.Object, context.Transaction);
        Assert.Same(_serviceProvider.Object, context.ServiceProvider);
        Assert.False(context.CancellationToken.IsCancellationRequested);
    }
}