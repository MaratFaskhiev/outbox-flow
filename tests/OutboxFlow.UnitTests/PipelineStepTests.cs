using Moq;
using OutboxFlow.Produce;
using Xunit;

namespace OutboxFlow.UnitTests;

public sealed class PipelineStepTests : IDisposable
{
    private readonly Mock<IPipelineStep<IProduceContext, string>> _nextStep = new();

    public void Dispose()
    {
        Mock.VerifyAll(_nextStep);
    }

    [Fact]
    public async Task RunAsync_AsyncAction_NextStepIsNull_RunsAction()
    {
        var input = 1;

        var isInvoked = false;
        var pipelineStep = new PipelineStep<IProduceContext, int, string>((_, _) =>
        {
            isInvoked = true;
            return new ValueTask<string>("result");
        }, null);

        await pipelineStep.RunAsync(input, Mock.Of<IProduceContext>());

        Assert.True(isInvoked);
    }

    [Fact]
    public async Task RunAsync_AsyncAction_NextStepIsNotNull_RunsNextStep()
    {
        var input = 1;
        var nextInput = "result";
        var context = Mock.Of<IProduceContext>();

        var pipelineStep = new PipelineStep<IProduceContext, int, string>(
            (_, _) => new ValueTask<string>(nextInput),
            _nextStep.Object);

        _nextStep.Setup(x => x.RunAsync(nextInput, context));

        await pipelineStep.RunAsync(input, context);
    }

    [Fact]
    public async Task RunAsync_SyncAction_NextStepIsNull_RunsAction()
    {
        var input = 1;

        var isInvoked = false;
        var pipelineStep = new PipelineStep<IProduceContext, int, string>((_, _) =>
        {
            isInvoked = true;
            return "result";
        }, null);

        await pipelineStep.RunAsync(input, Mock.Of<IProduceContext>());

        Assert.True(isInvoked);
    }

    [Fact]
    public async Task RunAsync_SyncAction_NextStepIsNotNull_RunsNextStep()
    {
        var input = 1;
        var nextInput = "result";
        var context = Mock.Of<IProduceContext>();

        var pipelineStep = new PipelineStep<IProduceContext, int, string>(
            (_, _) => nextInput,
            _nextStep.Object);

        _nextStep.Setup(x => x.RunAsync(nextInput, context));

        await pipelineStep.RunAsync(input, context);
    }
}