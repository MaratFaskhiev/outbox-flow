using Moq;
using OutboxFlow.Consume;
using OutboxFlow.Storage;
using Xunit;

namespace OutboxFlow.UnitTests.Consume;

public sealed class ConsumePipelineRegistryTests
{
    [Fact]
    public void GetPipeline_DestinationIsNull_DefaultPipelineIsNull_ThrowsInvalidOperationException()
    {
        var registry = new ConsumePipelineRegistry(
            new Dictionary<string, IPipelineStep<IConsumeContext, IOutboxMessage>>(),
            null);

        Assert.Throws<InvalidOperationException>(() => registry.GetPipeline());
    }

    [Fact]
    public void GetPipeline_DestinationIsNull_ReturnsDefaultPipeline()
    {
        var defaultPipeline = Mock.Of<IPipelineStep<IConsumeContext, IOutboxMessage>>();
        var registry = new ConsumePipelineRegistry(
            new Dictionary<string, IPipelineStep<IConsumeContext, IOutboxMessage>>(),
            defaultPipeline);

        var result = registry.GetPipeline();

        Assert.Same(defaultPipeline, result);
    }

    [Fact]
    public void GetPipeline_DestinationIsNotFound_DefaultPipelineIsNull_ThrowsInvalidOperationException()
    {
        const string destination = "test";
        var registry = new ConsumePipelineRegistry(
            new Dictionary<string, IPipelineStep<IConsumeContext, IOutboxMessage>>(),
            null);

        Assert.Throws<InvalidOperationException>(() => registry.GetPipeline(destination));
    }

    [Fact]
    public void GetPipeline_DestinationIsNotFound_ReturnsDefaultPipeline()
    {
        const string destination = "test";

        var defaultPipeline = Mock.Of<IPipelineStep<IConsumeContext, IOutboxMessage>>();
        var registry = new ConsumePipelineRegistry(
            new Dictionary<string, IPipelineStep<IConsumeContext, IOutboxMessage>>(),
            defaultPipeline);

        var result = registry.GetPipeline(destination);

        Assert.Same(defaultPipeline, result);
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

        Assert.Same(pipeline1, result);
    }
}