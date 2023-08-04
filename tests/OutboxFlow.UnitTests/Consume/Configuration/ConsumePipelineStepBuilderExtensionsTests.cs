using Moq;
using OutboxFlow.Consume;
using OutboxFlow.Consume.Configuration;
using Xunit;

namespace OutboxFlow.UnitTests.Consume.Configuration;

public sealed class ConsumePipelineStepBuilderExtensionsTests : IDisposable
{
    private readonly Mock<IConsumeAsyncMiddleware<string, int>> _asyncMiddleware;
    private readonly Mock<IConsumePipelineStepBuilder<int, string>> _builder;
    private readonly Mock<IConsumeContext> _consumeContext;
    private readonly Mock<IConsumeAsyncMiddleware<string, string>> _sameTypeAsyncMiddleware;
    private readonly Mock<IConsumeSyncMiddleware<string, string>> _sameTypeSyncMiddleware;
    private readonly Mock<IServiceProvider> _serviceProvider;
    private readonly Mock<IConsumeSyncMiddleware<string, int>> _syncMiddleware;

    public ConsumePipelineStepBuilderExtensionsTests()
    {
        _builder = new Mock<IConsumePipelineStepBuilder<int, string>>(MockBehavior.Strict);
        _asyncMiddleware = new Mock<IConsumeAsyncMiddleware<string, int>>(MockBehavior.Strict);
        _sameTypeAsyncMiddleware = new Mock<IConsumeAsyncMiddleware<string, string>>(MockBehavior.Strict);
        _syncMiddleware = new Mock<IConsumeSyncMiddleware<string, int>>(MockBehavior.Strict);
        _sameTypeSyncMiddleware = new Mock<IConsumeSyncMiddleware<string, string>>(MockBehavior.Strict);
        _consumeContext = new Mock<IConsumeContext>(MockBehavior.Strict);
        _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);

        _consumeContext.Setup(x => x.ServiceProvider)
            .Returns(_serviceProvider.Object);
    }

    public void Dispose()
    {
        Mock.VerifyAll(
            _builder,
            _asyncMiddleware,
            _sameTypeAsyncMiddleware,
            _syncMiddleware,
            _sameTypeSyncMiddleware,
            _consumeContext,
            _serviceProvider);
    }

    [Fact]
    public async Task AddAsyncStep_AsyncMiddleware_AddsMiddlewareActionToPipeline()
    {
        var input = "test";
        var output = 1;

        Func<string, IConsumeContext, ValueTask<int>> pipelineStepAction = null!;
        _builder.Setup(x => x.AddAsyncStep(It.IsAny<Func<string, IConsumeContext, ValueTask<int>>>()))
            .Returns((Func<string, IConsumeContext, ValueTask<int>> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IConsumePipelineStepBuilder<string, int>>();
            });

        _serviceProvider.Setup(x => x.GetService(typeof(IConsumeAsyncMiddleware<string, int>)))
            .Returns(_asyncMiddleware.Object);

        _asyncMiddleware.Setup(x => x.RunAsync(input, _consumeContext.Object))
            .ReturnsAsync(output);

        _builder.Object.AddAsyncStep<IConsumeAsyncMiddleware<string, int>, int, string, int>();

        var result = await pipelineStepAction!.Invoke(input, _consumeContext.Object);

        Assert.Equal(output, result);
    }

    [Fact]
    public void AddSyncStep_SyncMiddleware_AddsMiddlewareActionToPipeline()
    {
        var input = "test";
        var output = 1;

        Func<string, IConsumeContext, int> pipelineStepAction = null!;
        _builder.Setup(x => x.AddSyncStep(It.IsAny<Func<string, IConsumeContext, int>>()))
            .Returns((Func<string, IConsumeContext, int> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IConsumePipelineStepBuilder<string, int>>();
            });

        _serviceProvider.Setup(x => x.GetService(typeof(IConsumeSyncMiddleware<string, int>)))
            .Returns(_syncMiddleware.Object);

        _syncMiddleware.Setup(x => x.Run(input, _consumeContext.Object))
            .Returns(output);

        _builder.Object.AddSyncStep<IConsumeSyncMiddleware<string, int>, int, string, int>();

        var result = pipelineStepAction!.Invoke(input, _consumeContext.Object);

        Assert.Equal(output, result);
    }

    [Fact]
    public async Task AddAsyncStep_SameTypeAsyncMiddleware_AddsMiddlewareActionToPipeline()
    {
        var input = "test";
        var output = "test_output";

        Func<string, IConsumeContext, ValueTask<string>> pipelineStepAction = null!;
        _builder.Setup(x => x.AddAsyncStep(It.IsAny<Func<string, IConsumeContext, ValueTask<string>>>()))
            .Returns((Func<string, IConsumeContext, ValueTask<string>> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IConsumePipelineStepBuilder<string, string>>();
            });

        _serviceProvider.Setup(x => x.GetService(typeof(IConsumeAsyncMiddleware<string, string>)))
            .Returns(_sameTypeAsyncMiddleware.Object);

        _sameTypeAsyncMiddleware.Setup(x => x.RunAsync(input, _consumeContext.Object))
            .ReturnsAsync(output);

        _builder.Object.AddAsyncStep<IConsumeAsyncMiddleware<string, string>, int, string>();

        var result = await pipelineStepAction!.Invoke(input, _consumeContext.Object);

        Assert.Equal(output, result);
    }

    [Fact]
    public void AddSyncStep_SameTypeSyncMiddleware_AddsMiddlewareActionToPipeline()
    {
        var input = "test";
        var output = "test_output";

        Func<string, IConsumeContext, string> pipelineStepAction = null!;
        _builder.Setup(x => x.AddSyncStep(It.IsAny<Func<string, IConsumeContext, string>>()))
            .Returns((Func<string, IConsumeContext, string> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IConsumePipelineStepBuilder<string, string>>();
            });

        _serviceProvider.Setup(x => x.GetService(typeof(IConsumeSyncMiddleware<string, string>)))
            .Returns(_sameTypeSyncMiddleware.Object);

        _sameTypeSyncMiddleware.Setup(x => x.Run(input, _consumeContext.Object))
            .Returns(output);

        _builder.Object.AddSyncStep<IConsumeSyncMiddleware<string, string>, int, string>();

        var result = pipelineStepAction!.Invoke(input, _consumeContext.Object);

        Assert.Equal(output, result);
    }
}