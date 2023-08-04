using Microsoft.Extensions.DependencyInjection;
using Moq;
using OutboxFlow.Configuration;
using OutboxFlow.Storage.Configuration;
using Xunit;

namespace OutboxFlow.UnitTests.Configuration;

public sealed class OutboxBuilderTests : IDisposable
{
    private readonly OutboxBuilder _builder;
    private readonly Mock<IOutboxStorageRegistrar> _outboxStorageRegistrar;
    private readonly Mock<IServiceCollection> _services;

    public OutboxBuilderTests()
    {
        _services = new Mock<IServiceCollection>(MockBehavior.Strict);
        _outboxStorageRegistrar = new Mock<IOutboxStorageRegistrar>(MockBehavior.Strict);

        _builder = new OutboxBuilder();
    }

    public void Dispose()
    {
        Mock.VerifyAll(_services, _outboxStorageRegistrar);
    }

    [Fact]
    public void AddProducer_IsAlreadyConfigured_ThrowsInvalidOperationException()
    {
        _builder.AddProducer(_ => { });

        Assert.Throws<InvalidOperationException>(() => _builder.AddProducer(_ => { }));
    }

    [Fact]
    public void Build_ProducerIsConfigured_InvokesConfigurationAction()
    {
        _services.Setup(x => x.Count).Returns(0);
        _services.Setup(x => x.Add(It.IsAny<ServiceDescriptor>()));
        _outboxStorageRegistrar.Setup(x => x.Register(_services.Object));

        var isInvoked = false;
        _builder.AddProducer(producerBuilder =>
        {
            producerBuilder.OutboxStorageRegistrar = _outboxStorageRegistrar.Object;
            isInvoked = true;
        });

        _builder.Build(_services.Object);

        Assert.True(isInvoked);
    }

    [Fact]
    public void AddConsumer_IsAlreadyConfigured_ThrowsInvalidOperationException()
    {
        _builder.AddConsumer(_ => { });

        Assert.Throws<InvalidOperationException>(() => _builder.AddConsumer(_ => { }));
    }

    [Fact]
    public void Build_ConsumerIsConfigured_InvokesConfigurationAction()
    {
        _services.Setup(x => x.Count).Returns(0);
        _services.Setup(x => x.Add(It.IsAny<ServiceDescriptor>()));
        _outboxStorageRegistrar.Setup(x => x.Register(_services.Object));

        var isInvoked = false;
        _builder.AddConsumer(consumerBuilder =>
        {
            consumerBuilder.OutboxStorageRegistrar = _outboxStorageRegistrar.Object;
            isInvoked = true;
        });

        _builder.Build(_services.Object);

        Assert.True(isInvoked);
    }

    [Fact]
    public void Build_IsNotConfigured_DoesNothing()
    {
        _builder.Build(_services.Object);
    }
}