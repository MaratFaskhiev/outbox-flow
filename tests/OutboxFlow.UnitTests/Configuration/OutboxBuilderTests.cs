using FluentAssertions;
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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddComponent_IsAlreadyConfigured_ThrowsInvalidOperationException(bool isProducer)
    {
        if (isProducer)
        {
            _builder.AddProducer(_ => { });
            FluentActions.Invoking(() => _builder.AddProducer(_ => { })).Should().Throw<InvalidOperationException>();
        }
        else
        {
            _builder.AddConsumer(_ => { });
            FluentActions.Invoking(() => _builder.AddConsumer(_ => { })).Should().Throw<InvalidOperationException>();
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Build_ComponentIsConfigured_InvokesConfigurationAction(bool isProducer)
    {
        _services.Setup(x => x.Count).Returns(0);
        _services.Setup(x => x.Add(It.IsAny<ServiceDescriptor>()));
        _outboxStorageRegistrar.Setup(x => x.Register(_services.Object));

        var isInvoked = false;
        if (isProducer)
            _builder.AddProducer(producerBuilder =>
            {
                producerBuilder.OutboxStorageRegistrar = _outboxStorageRegistrar.Object;
                isInvoked = true;
            });
        else
            _builder.AddConsumer(consumerBuilder =>
            {
                consumerBuilder.OutboxStorageRegistrar = _outboxStorageRegistrar.Object;
                isInvoked = true;
            });

        _builder.Build(_services.Object);

        isInvoked.Should().BeTrue();
    }

    [Fact]
    public void Build_IsNotConfigured_DoesNothing()
    {
        _builder.Build(_services.Object);
    }

    [Fact]
    public void Build_BothProducerAndConsumerAreConfigured_InvokesBothConfigurationActions()
    {
        _services.Setup(x => x.Count).Returns(0);
        _services.Setup(x => x.Add(It.IsAny<ServiceDescriptor>()));
        _outboxStorageRegistrar.Setup(x => x.Register(_services.Object));

        var producerInvoked = false;
        var consumerInvoked = false;

        _builder.AddProducer(producerBuilder =>
        {
            producerBuilder.OutboxStorageRegistrar = _outboxStorageRegistrar.Object;
            producerInvoked = true;
        });
        _builder.AddConsumer(consumerBuilder =>
        {
            consumerBuilder.OutboxStorageRegistrar = _outboxStorageRegistrar.Object;
            consumerInvoked = true;
        });

        _builder.Build(_services.Object);

        producerInvoked.Should().BeTrue();
        consumerInvoked.Should().BeTrue();
    }
}