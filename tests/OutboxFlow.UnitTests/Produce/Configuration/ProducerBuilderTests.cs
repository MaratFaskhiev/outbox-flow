using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OutboxFlow.Produce;
using OutboxFlow.Produce.Configuration;
using OutboxFlow.Storage.Configuration;
using Xunit;

namespace OutboxFlow.UnitTests.Produce.Configuration;

public sealed class ProducerBuilderTests : IDisposable
{
    private readonly ProducerBuilder _builder;
    private readonly Mock<IOutboxStorageRegistrar> _outboxStorageRegistrar;
    private readonly Mock<IServiceCollection> _services;

    public ProducerBuilderTests()
    {
        _services = new Mock<IServiceCollection>(MockBehavior.Strict);
        _outboxStorageRegistrar = new Mock<IOutboxStorageRegistrar>(MockBehavior.Strict);

        _builder = new ProducerBuilder();
    }

    public void Dispose()
    {
        Mock.VerifyAll(_services, _outboxStorageRegistrar);
    }

    [Fact]
    public void ForMessage_MessageTypePipelineIsAlreadyConfigured_ThrowsInvalidOperationException()
    {
        _builder.ForMessage<string>(_ => { });

        FluentActions.Invoking(() => _builder.ForMessage<string>(_ => { })).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Build_OutboxStorageRegistrarIsNotSet_ThrowsInvalidOperationException()
    {
        FluentActions.Invoking(() => _builder.Build(_services.Object)).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Build_OutboxStorageRegistrarIsSet_RegistersProducerDependencies()
    {
        _builder.OutboxStorageRegistrar = _outboxStorageRegistrar.Object;

        _outboxStorageRegistrar.Setup(x => x.Register(_services.Object));

        _services.Setup(x => x.Count).Returns(0);
        _services.Setup(x => x.Add(It.IsAny<ServiceDescriptor>()));

        _builder.Build(_services.Object);

        _outboxStorageRegistrar.Verify(x => x.Register(_services.Object), Times.Once);

        _services.Verify(x => x.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(IProducePipelineRegistry))),
            Times.Once);
        _services.Verify(
            x => x.Add(It.Is<ServiceDescriptor>(d =>
                d.ServiceType == typeof(IProducer) && d.ImplementationFactory != null)),
            Times.Once);
    }

    [Fact]
    public async Task Build_MessageTypePipelineIsConfigured_ConfiguresMessageTypePipeline()
    {
        _builder.OutboxStorageRegistrar = _outboxStorageRegistrar.Object;

        var isInvoked = false;
        _builder.ForMessage<string>(builder =>
        {
            builder.AddSyncStep((_, _) =>
            {
                isInvoked = true;

                return 1;
            });
        });

        _outboxStorageRegistrar.Setup(x => x.Register(_services.Object));

        _services.Setup(x => x.Count).Returns(0);
        _services.Setup(x => x.Add(It.IsAny<ServiceDescriptor>()));

        IProducePipelineRegistry? pipelineRegistry = null;
        _services.Setup(x => x.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(IProducePipelineRegistry))))
            .Callback((ServiceDescriptor descriptor) =>
            {
                pipelineRegistry =
                    (IProducePipelineRegistry?) descriptor.ImplementationInstance;
            });

        _builder.Build(_services.Object);

        pipelineRegistry.Should().NotBeNull();

        var pipeline = pipelineRegistry!.GetPipeline<string>();

        pipeline.Should().NotBeNull();

        await pipeline!.RunAsync(string.Empty, Mock.Of<IProduceContext>());

        isInvoked.Should().BeTrue();
    }
}