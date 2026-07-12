using FluentAssertions;
using Moq;
using OutboxFlow.Produce;
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

    [Fact]
    public async Task RunAsync_CancellationTokenIsCancelled_ThrowsOperationCanceledException()
    {
        var cancellationToken = new CancellationToken(true);
        var produceContext = Mock.Of<IProduceContext>(x => x.CancellationToken == cancellationToken);

        var stepMock = new Mock<IPipelineStep<IProduceContext, string>>(MockBehavior.Strict);
        stepMock
            .Setup(x => x.RunAsync(It.IsAny<string>(), produceContext))
            .Callback<string, IProduceContext>((_, ctx) => ctx.CancellationToken.ThrowIfCancellationRequested())
            .Returns(new ValueTask());

        var pipeline = new Pipeline<IProduceContext, string>(stepMock.Object);

        await FluentActions
            .Awaiting(() => pipeline.RunAsync("message", produceContext).AsTask())
            .Should().ThrowAsync<OperationCanceledException>();

        Mock.VerifyAll(stepMock);
    }
}