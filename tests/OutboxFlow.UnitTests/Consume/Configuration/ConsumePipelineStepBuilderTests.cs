using FluentAssertions;
using Moq;
using OutboxFlow.Consume;
using OutboxFlow.Consume.Configuration;
using Xunit;

namespace OutboxFlow.UnitTests.Consume.Configuration;

public sealed class ConsumePipelineStepBuilderTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Build_PipelineRunsAction(bool isAsync)
    {
        var isInvoked = false;
        ConsumePipelineStepBuilder<int, int> builder;

        if (isAsync)
            builder = new ConsumePipelineStepBuilder<int, int>((_, _) =>
            {
                isInvoked = true;
                return new ValueTask<int>(1);
            });
        else
            builder = new ConsumePipelineStepBuilder<int, int>((_, _) =>
            {
                isInvoked = true;
                return 1;
            });

        var pipelineStep = builder.Build();
        await pipelineStep.RunAsync(1, Mock.Of<IConsumeContext>());

        isInvoked.Should().BeTrue();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AddStep_StepIsAlreadyConfigured_ThrowsInvalidOperationException(bool isAsync)
    {
        var builder = new ConsumePipelineStepBuilder<int, int>((x, _) => x);

        if (isAsync)
        {
            builder.AddAsyncStep((_, _) => new ValueTask<int>(1));

            FluentActions.Invoking(() => builder.AddAsyncStep((_, _) => new ValueTask<int>(1)))
                .Should().Throw<InvalidOperationException>();
        }
        else
        {
            builder.AddSyncStep((_, _) => 1);

            FluentActions.Invoking(() => builder.AddSyncStep((_, _) => 1))
                .Should().Throw<InvalidOperationException>();
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Build_AddStepIsInvoked_PipelineRunsAction(bool isAsync)
    {
        var builder = new ConsumePipelineStepBuilder<int, int>((x, _) => x);

        var isInvoked = false;
        if (isAsync)
            builder.AddAsyncStep((_, _) =>
            {
                isInvoked = true;
                return new ValueTask<int>(1);
            });
        else
            builder.AddSyncStep((_, _) =>
            {
                isInvoked = true;
                return 1;
            });

        var pipelineStep = builder.Build();
        await pipelineStep.RunAsync(1, Mock.Of<IConsumeContext>());

        isInvoked.Should().BeTrue();
    }
}