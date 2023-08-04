using Moq;
using OutboxFlow.Produce;
using OutboxFlow.Produce.Configuration;
using Xunit;

namespace OutboxFlow.UnitTests.Produce.Configuration;

public sealed class ProducePipelineBuilderTests
{
    private readonly ProducePipelineBuilder<int> _builder;

    public ProducePipelineBuilderTests()
    {
        _builder = new ProducePipelineBuilder<int>();
    }

    [Fact]
    public void AddAsyncStep_StepIsAlreadyConfigured_ThrowsInvalidOperationException()
    {
        _builder.AddAsyncStep((_, _) => new ValueTask<int>(1));

        Assert.Throws<InvalidOperationException>(() => _builder.AddAsyncStep((_, _) => new ValueTask<int>(1)));
    }

    [Fact]
    public void AddSyncStep_StepIsAlreadyConfigured_ThrowsInvalidOperationException()
    {
        _builder.AddSyncStep((_, _) => 1);

        Assert.Throws<InvalidOperationException>(() => _builder.AddSyncStep((_, _) => 1));
    }

    [Fact]
    public async Task Build_SyncStepIsConfigured_PipelineRunsAction()
    {
        var isInvoked = false;
        _builder.AddSyncStep((_, _) =>
        {
            isInvoked = true;
            return 1;
        });

        var pipelineStep = _builder.Build();
        await pipelineStep.RunAsync(1, Mock.Of<IProduceContext>());

        Assert.True(isInvoked);
    }

    [Fact]
    public async Task Build_AsyncStepIsConfigured_PipelineRunsAction()
    {
        var isInvoked = false;
        _builder.AddAsyncStep((_, _) =>
        {
            isInvoked = true;
            return new ValueTask<int>(1);
        });

        var pipelineStep = _builder.Build();
        await pipelineStep.RunAsync(1, Mock.Of<IProduceContext>());

        Assert.True(isInvoked);
    }
}