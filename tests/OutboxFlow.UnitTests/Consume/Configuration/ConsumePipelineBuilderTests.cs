using Moq;
using OutboxFlow.Consume;
using OutboxFlow.Consume.Configuration;
using OutboxFlow.Storage;
using Xunit;

namespace OutboxFlow.UnitTests.Consume.Configuration;

public class ConsumePipelineBuilderTests
{
    private readonly ConsumePipelineBuilder _builder;

    public ConsumePipelineBuilderTests()
    {
        _builder = new ConsumePipelineBuilder();
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
        await pipelineStep.RunAsync(Mock.Of<IOutboxMessage>(), Mock.Of<IConsumeContext>());

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
        await pipelineStep.RunAsync(Mock.Of<IOutboxMessage>(), Mock.Of<IConsumeContext>());

        Assert.True(isInvoked);
    }
}