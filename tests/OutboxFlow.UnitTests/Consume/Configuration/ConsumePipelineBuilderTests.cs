using FluentAssertions;
using Moq;
using OutboxFlow.Consume;
using OutboxFlow.Consume.Configuration;
using OutboxFlow.Storage;
using Xunit;

namespace OutboxFlow.UnitTests.Consume.Configuration;

public sealed class ConsumePipelineBuilderTests
{
    private readonly ConsumePipelineBuilder _builder;

    public ConsumePipelineBuilderTests()
    {
        _builder = new ConsumePipelineBuilder();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AddStep_StepIsAlreadyConfigured_ThrowsInvalidOperationException(bool isAsync)
    {
        if (isAsync)
        {
            _builder.AddAsyncStep((_, _) => new ValueTask<int>(1));

            FluentActions.Invoking(() => _builder.AddAsyncStep((_, _) => new ValueTask<int>(1)))
                .Should().Throw<InvalidOperationException>();
        }
        else
        {
            _builder.AddSyncStep((_, _) => 1);

            FluentActions.Invoking(() => _builder.AddSyncStep((_, _) => 1))
                .Should().Throw<InvalidOperationException>();
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Build_StepIsConfigured_PipelineRunsAction(bool isAsync)
    {
        var isInvoked = false;
        if (isAsync)
            _builder.AddAsyncStep((_, _) =>
            {
                isInvoked = true;
                return new ValueTask<int>(1);
            });
        else
            _builder.AddSyncStep((_, _) =>
            {
                isInvoked = true;
                return 1;
            });

        var pipelineStep = _builder.Build();
        await pipelineStep.RunAsync(Mock.Of<IOutboxMessage>(), Mock.Of<IConsumeContext>());

        isInvoked.Should().BeTrue();
    }
}