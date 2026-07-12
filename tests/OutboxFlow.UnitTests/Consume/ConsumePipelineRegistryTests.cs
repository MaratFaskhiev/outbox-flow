using FluentAssertions;
using Moq;
using OutboxFlow.Consume;
using OutboxFlow.Storage;
using Xunit;

namespace OutboxFlow.UnitTests.Consume;

public sealed class ConsumePipelineRegistryTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("test")]
    public void GetPipeline_DefaultPipelineIsNull_ThrowsInvalidOperationException(string? destination)
    {
        var registry = new ConsumePipelineRegistry(
            new Dictionary<string, IPipelineStep<IConsumeContext, IOutboxMessage>>(),
            null);

        var action = destination is null
            ? (Action) (() => registry.GetPipeline())
            : () => registry.GetPipeline(destination);

        action.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("test")]
    public void GetPipeline_DefaultPipelineIsSet_ReturnsDefaultPipeline(string? destination)
    {
        var defaultPipeline = Mock.Of<IPipelineStep<IConsumeContext, IOutboxMessage>>();
        var registry = new ConsumePipelineRegistry(
            new Dictionary<string, IPipelineStep<IConsumeContext, IOutboxMessage>>(),
            defaultPipeline);

        var result = destination is null
            ? registry.GetPipeline()
            : registry.GetPipeline(destination);

        result.Should().BeSameAs(defaultPipeline);
    }

    [Fact]
    public void GetPipeline_DestinationIsFound_ReturnsPipeline()
    {
        const string destination1 = "test1";
        const string destination2 = "test2";

        var defaultPipeline = Mock.Of<IPipelineStep<IConsumeContext, IOutboxMessage>>();
        var pipeline1 = Mock.Of<IPipelineStep<IConsumeContext, IOutboxMessage>>();
        var pipeline2 = Mock.Of<IPipelineStep<IConsumeContext, IOutboxMessage>>();

        var registry = new ConsumePipelineRegistry(
            new Dictionary<string, IPipelineStep<IConsumeContext, IOutboxMessage>>
            {
                {destination1, pipeline1},
                {destination2, pipeline2}
            },
            defaultPipeline);

        var result = registry.GetPipeline(destination1);

        result.Should().BeSameAs(pipeline1);
    }

    [Fact]
    public void GetPipeline_DestinationIsExplicitNull_ThrowsArgumentNullException()
    {
        var registry = new ConsumePipelineRegistry(
            new Dictionary<string, IPipelineStep<IConsumeContext, IOutboxMessage>>(),
            Mock.Of<IPipelineStep<IConsumeContext, IOutboxMessage>>());

        FluentActions.Invoking(() => registry.GetPipeline(null!)).Should().Throw<ArgumentNullException>();
    }
}