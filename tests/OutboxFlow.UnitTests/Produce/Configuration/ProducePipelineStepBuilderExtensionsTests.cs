using Moq;
using OutboxFlow.Produce;
using OutboxFlow.Produce.Configuration;
using OutboxFlow.Storage;
using Xunit;

namespace OutboxFlow.UnitTests.Produce.Configuration;

public sealed class ProducePipelineStepBuilderExtensionsTests : IDisposable
{
    private readonly Mock<IProduceAsyncMiddleware<string, int>> _asyncMiddleware;
    private readonly Mock<IProducePipelineStepBuilder<int, string>> _builder;
    private readonly Mock<IProduceContext> _produceContext;
    private readonly Mock<IProduceAsyncMiddleware<string, string>> _sameTypeAsyncMiddleware;
    private readonly Mock<IProducePipelineStepBuilder<string, string>> _sameTypeBuilder;
    private readonly Mock<IProduceSyncMiddleware<string, string>> _sameTypeSyncMiddleware;
    private readonly Mock<IServiceProvider> _serviceProvider;
    private readonly Mock<IProduceSyncMiddleware<string, int>> _syncMiddleware;

    public ProducePipelineStepBuilderExtensionsTests()
    {
        _builder = new Mock<IProducePipelineStepBuilder<int, string>>(MockBehavior.Strict);
        _sameTypeBuilder = new Mock<IProducePipelineStepBuilder<string, string>>(MockBehavior.Strict);
        _asyncMiddleware = new Mock<IProduceAsyncMiddleware<string, int>>(MockBehavior.Strict);
        _sameTypeAsyncMiddleware = new Mock<IProduceAsyncMiddleware<string, string>>(MockBehavior.Strict);
        _syncMiddleware = new Mock<IProduceSyncMiddleware<string, int>>(MockBehavior.Strict);
        _sameTypeSyncMiddleware = new Mock<IProduceSyncMiddleware<string, string>>(MockBehavior.Strict);
        _produceContext = new Mock<IProduceContext>(MockBehavior.Strict);
        _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
    }

    public void Dispose()
    {
        Mock.VerifyAll(
            _builder,
            _sameTypeBuilder,
            _asyncMiddleware,
            _sameTypeAsyncMiddleware,
            _syncMiddleware,
            _sameTypeSyncMiddleware,
            _produceContext,
            _serviceProvider);
    }

    [Fact]
    public async Task AddAsyncStep_AsyncMiddleware_AddsMiddlewareActionToPipeline()
    {
        var input = "test";
        var output = 1;

        Func<string, IProduceContext, ValueTask<int>> pipelineStepAction = null!;
        _builder.Setup(x => x.AddAsyncStep(It.IsAny<Func<string, IProduceContext, ValueTask<int>>>()))
            .Returns((Func<string, IProduceContext, ValueTask<int>> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IProducePipelineStepBuilder<string, int>>();
            });

        _serviceProvider.Setup(x => x.GetService(typeof(IProduceAsyncMiddleware<string, int>)))
            .Returns(_asyncMiddleware.Object);

        _asyncMiddleware.Setup(x => x.RunAsync(input, _produceContext.Object))
            .ReturnsAsync(output);

        _produceContext.Setup(x => x.ServiceProvider)
            .Returns(_serviceProvider.Object);

        _builder.Object.AddAsyncStep<IProduceAsyncMiddleware<string, int>, int, string, int>();

        var result = await pipelineStepAction!.Invoke(input, _produceContext.Object);

        Assert.Equal(output, result);
    }

    [Fact]
    public async Task AddAsyncStep_SameTypeAsyncMiddleware_AddsMiddlewareActionToPipeline()
    {
        var input = "test";
        var output = "test_output";

        Func<string, IProduceContext, ValueTask<string>> pipelineStepAction = null!;
        _builder.Setup(x => x.AddAsyncStep(It.IsAny<Func<string, IProduceContext, ValueTask<string>>>()))
            .Returns((Func<string, IProduceContext, ValueTask<string>> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IProducePipelineStepBuilder<string, string>>();
            });

        _serviceProvider.Setup(x => x.GetService(typeof(IProduceAsyncMiddleware<string, string>)))
            .Returns(_sameTypeAsyncMiddleware.Object);

        _sameTypeAsyncMiddleware.Setup(x => x.RunAsync(input, _produceContext.Object))
            .ReturnsAsync(output);

        _produceContext.Setup(x => x.ServiceProvider)
            .Returns(_serviceProvider.Object);

        _builder.Object.AddAsyncStep<IProduceAsyncMiddleware<string, string>, int, string>();

        var result = await pipelineStepAction!.Invoke(input, _produceContext.Object);

        Assert.Equal(output, result);
    }

    [Fact]
    public async Task AddAsyncStep_SameTypeBuilderAndAsyncMiddleware_AddsMiddlewareActionToPipeline()
    {
        var input = "test";
        var output = "test_output";

        Func<string, IProduceContext, ValueTask<string>> pipelineStepAction = null!;
        _sameTypeBuilder.Setup(x => x.AddAsyncStep(It.IsAny<Func<string, IProduceContext, ValueTask<string>>>()))
            .Returns((Func<string, IProduceContext, ValueTask<string>> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IProducePipelineStepBuilder<string, string>>();
            });

        _serviceProvider.Setup(x => x.GetService(typeof(IProduceAsyncMiddleware<string, string>)))
            .Returns(_sameTypeAsyncMiddleware.Object);

        _sameTypeAsyncMiddleware.Setup(x => x.RunAsync(input, _produceContext.Object))
            .ReturnsAsync(output);

        _produceContext.Setup(x => x.ServiceProvider)
            .Returns(_serviceProvider.Object);

        _sameTypeBuilder.Object.AddAsyncStep<IProduceAsyncMiddleware<string, string>, string>();

        var result = await pipelineStepAction!.Invoke(input, _produceContext.Object);

        Assert.Equal(output, result);
    }

    [Fact]
    public void AddSyncStep_SyncMiddleware_AddsMiddlewareActionToPipeline()
    {
        var input = "test";
        var output = 1;

        Func<string, IProduceContext, int> pipelineStepAction = null!;
        _builder.Setup(x => x.AddSyncStep(It.IsAny<Func<string, IProduceContext, int>>()))
            .Returns((Func<string, IProduceContext, int> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IProducePipelineStepBuilder<string, int>>();
            });

        _serviceProvider.Setup(x => x.GetService(typeof(IProduceSyncMiddleware<string, int>)))
            .Returns(_syncMiddleware.Object);

        _syncMiddleware.Setup(x => x.Run(input, _produceContext.Object))
            .Returns(output);

        _produceContext.Setup(x => x.ServiceProvider)
            .Returns(_serviceProvider.Object);

        _builder.Object.AddSyncStep<IProduceSyncMiddleware<string, int>, int, string, int>();

        var result = pipelineStepAction!.Invoke(input, _produceContext.Object);

        Assert.Equal(output, result);
    }

    [Fact]
    public void AddSyncStep_SameTypeSyncMiddleware_AddsMiddlewareActionToPipeline()
    {
        var input = "test";
        var output = "test_output";

        Func<string, IProduceContext, string> pipelineStepAction = null!;
        _builder.Setup(x => x.AddSyncStep(It.IsAny<Func<string, IProduceContext, string>>()))
            .Returns((Func<string, IProduceContext, string> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IProducePipelineStepBuilder<string, string>>();
            });

        _serviceProvider.Setup(x => x.GetService(typeof(IProduceSyncMiddleware<string, string>)))
            .Returns(_sameTypeSyncMiddleware.Object);

        _sameTypeSyncMiddleware.Setup(x => x.Run(input, _produceContext.Object))
            .Returns(output);

        _produceContext.Setup(x => x.ServiceProvider)
            .Returns(_serviceProvider.Object);

        _builder.Object.AddSyncStep<IProduceSyncMiddleware<string, string>, int, string>();

        var result = pipelineStepAction!.Invoke(input, _produceContext.Object);

        Assert.Equal(output, result);
    }

    [Fact]
    public void AddSyncStep_SameTypeBuilderAndSyncMiddleware_AddsMiddlewareActionToPipeline()
    {
        var input = "test";
        var output = "test_output";

        Func<string, IProduceContext, string> pipelineStepAction = null!;
        _sameTypeBuilder.Setup(x => x.AddSyncStep(It.IsAny<Func<string, IProduceContext, string>>()))
            .Returns((Func<string, IProduceContext, string> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IProducePipelineStepBuilder<string, string>>();
            });

        _serviceProvider.Setup(x => x.GetService(typeof(IProduceSyncMiddleware<string, string>)))
            .Returns(_sameTypeSyncMiddleware.Object);

        _sameTypeSyncMiddleware.Setup(x => x.Run(input, _produceContext.Object))
            .Returns(output);

        _produceContext.Setup(x => x.ServiceProvider)
            .Returns(_serviceProvider.Object);

        _sameTypeBuilder.Object.AddSyncStep<IProduceSyncMiddleware<string, string>, string>();

        var result = pipelineStepAction!.Invoke(input, _produceContext.Object);

        Assert.Equal(output, result);
    }

    [Fact]
    public void SetKey_SetsContextKey()
    {
        var input = "test";
        var key = Guid.NewGuid().ToByteArray();

        Func<string, IProduceContext, string> pipelineStepAction = null!;
        _builder.Setup(x => x.AddSyncStep(It.IsAny<Func<string, IProduceContext, string>>()))
            .Returns((Func<string, IProduceContext, string> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IProducePipelineStepBuilder<string, string>>();
            });

        _produceContext.SetupSet(x => x.Key = key);

        _builder.Object.SetKey(_ => key);

        var result = pipelineStepAction!.Invoke(input, _produceContext.Object);

        Assert.Equal(input, result);
    }

    [Fact]
    public void SetDestination_SetsContextDestination()
    {
        var input = "test";
        var destination = "destination";

        Func<string, IProduceContext, string> pipelineStepAction = null!;
        _builder.Setup(x => x.AddSyncStep(It.IsAny<Func<string, IProduceContext, string>>()))
            .Returns((Func<string, IProduceContext, string> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IProducePipelineStepBuilder<string, string>>();
            });

        _produceContext.SetupSet(x => x.Destination = destination);

        _builder.Object.SetDestination(destination);

        var result = pipelineStepAction!.Invoke(input, _produceContext.Object);

        Assert.Equal(input, result);
    }

    [Fact]
    public async Task Save_SavesToOutboxStorage()
    {
        var input = "test";

        Func<string, IProduceContext, ValueTask<string>> pipelineStepAction = null!;
        _builder.Setup(x => x.AddAsyncStep(It.IsAny<Func<string, IProduceContext, ValueTask<string>>>()))
            .Returns((Func<string, IProduceContext, ValueTask<string>> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IProducePipelineStepBuilder<string, string>>();
            });

        var outboxStorage = new Mock<IOutboxStorage>(MockBehavior.Strict);

        _serviceProvider.Setup(x => x.GetService(typeof(IOutboxStorage)))
            .Returns(outboxStorage.Object);

        outboxStorage.Setup(x => x.SaveAsync(_produceContext.Object))
            .Returns(new ValueTask());

        _produceContext.Setup(x => x.ServiceProvider)
            .Returns(_serviceProvider.Object);

        _builder.Object.Save();

        await pipelineStepAction!.Invoke(input, _produceContext.Object);

        outboxStorage.Verify(x => x.SaveAsync(_produceContext.Object), Times.Once);
    }
}