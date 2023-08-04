using Moq;
using OutboxFlow.Produce;
using Xunit;

namespace OutboxFlow.UnitTests.Produce;

public sealed class ProducePipelineRegistryTests : IDisposable
{
    private readonly Mock<IPipelineStep<IProduceContext, string>> _pipeline;
    private readonly ProducePipelineRegistry _registry;

    public ProducePipelineRegistryTests()
    {
        _pipeline = new Mock<IPipelineStep<IProduceContext, string>>(MockBehavior.Strict);

        _registry = new ProducePipelineRegistry(new Dictionary<Type, object>
        {
            {typeof(string), _pipeline.Object}
        });
    }

    public void Dispose()
    {
        Mock.VerifyAll(_pipeline);
    }

    [Fact]
    public void GetPipeline_TypeIsNotRegistered_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => _registry.GetPipeline<int>());
    }

    [Fact]
    public void GetPipeline_TypeRegistered_ReturnsPipeline()
    {
        var result = _registry.GetPipeline<string>();

        Assert.Same(_pipeline.Object, result);
    }
}