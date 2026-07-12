using FluentAssertions;
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
        await pipelineStep.RunAsync(1, Mock.Of<IProduceContext>());

        isInvoked.Should().BeTrue();
    }
}