using Moq;
using Xunit;

namespace OutboxFlow.UnitTests;

public sealed class PipelineTests : IDisposable
{
    private readonly Mock<IPipelineStep<int, string>> _pipelineStep;

    public PipelineTests()
    {
        _pipelineStep = new Mock<IPipelineStep<int, string>>(MockBehavior.Strict);
    }

    public void Dispose()
    {
        Mock.VerifyAll(_pipelineStep);
    }

    [Fact]
    private async Task Run_StepIsNull_DoesNothing()
    {
        var pipeline = new Pipeline<int, string>(null);

        await pipeline.RunAsync("message", 1);
    }

    [Fact]
    private async Task Run_StepIsNotNull_RunsStep()
    {
        var message = "message";
        var context = 1;

        _pipelineStep.Setup(x => x.RunAsync(message, context)).Returns(new ValueTask());

        var pipeline = new Pipeline<int, string>(_pipelineStep.Object);

        await pipeline.RunAsync(message, context);
    }
}