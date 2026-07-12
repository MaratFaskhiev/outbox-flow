using System.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using OutboxFlow.Consume;
using OutboxFlow.Consume.Configuration;
using OutboxFlow.Storage;
using OutboxFlow.Storage.Configuration;
using Xunit;

namespace OutboxFlow.UnitTests.Consume.Configuration;

public sealed class ConsumerBuilderTests : IDisposable
{
    private readonly ConsumerBuilder _builder;
    private readonly Mock<IOutboxStorageRegistrar> _outboxStorageRegistrar;
    private readonly Mock<IServiceCollection> _services;

    public ConsumerBuilderTests()
    {
        _services = new Mock<IServiceCollection>(MockBehavior.Strict);
        _outboxStorageRegistrar = new Mock<IOutboxStorageRegistrar>(MockBehavior.Strict);

        _builder = new ConsumerBuilder();
    }

    public void Dispose()
    {
        Mock.VerifyAll(_services, _outboxStorageRegistrar);
    }

    [Fact]
    public void SetDefaultRoute_DefaultRouteIsAlreadySet_ThrowsInvalidOperationException()
    {
        _builder.SetDefaultRoute(_ => { });

        FluentActions.Invoking(() => _builder.SetDefaultRoute(_ => { })).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddRoute_RouteIsAlreadySet_ThrowsInvalidOperationException()
    {
        const string destination = "test";
        _builder.AddRoute(destination, _ => { });

        FluentActions.Invoking(() => _builder.AddRoute(destination, _ => { })).Should()
            .Throw<InvalidOperationException>();
    }

    [Fact]
    public void Build_OutboxStorageRegistrarIsNotSet_ThrowsInvalidOperationException()
    {
        FluentActions.Invoking(() => _builder.Build(_services.Object)).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Build_OutboxStorageRegistrarIsSet_RegistersOutboxStorage()
    {
        _builder.OutboxStorageRegistrar = _outboxStorageRegistrar.Object;

        _outboxStorageRegistrar.Setup(x => x.Register(_services.Object));

        _services.Setup(x => x.Count).Returns(0);
        _services.Setup(x => x.Add(It.IsAny<ServiceDescriptor>()));

        _builder.Build(_services.Object);

        _outboxStorageRegistrar.Verify(x => x.Register(_services.Object), Times.Once);
    }

    [Fact]
    public void Build_OptionsAreSet_ConfiguresOutboxStorageConsumerOptions()
    {
        _builder.OutboxStorageRegistrar = _outboxStorageRegistrar.Object;
        _builder.IsolationLevel = IsolationLevel.Serializable;
        _builder.BatchSize = 3;
        _builder.ConsumeDelay = TimeSpan.FromDays(3);
        _builder.Timeout = TimeSpan.FromDays(4);

        _outboxStorageRegistrar.Setup(x => x.Register(_services.Object));

        _services.Setup(x => x.Count).Returns(0);
        _services.Setup(x => x.Add(It.IsAny<ServiceDescriptor>()));

        ConfigureNamedOptions<OutboxStorageConsumerOptions>? configureOptions = null;
        _services.Setup(x =>
                x.Add(It.Is<ServiceDescriptor>(d =>
                    d.ServiceType == typeof(IConfigureOptions<OutboxStorageConsumerOptions>))))
            .Callback((ServiceDescriptor descriptor) =>
            {
                configureOptions =
                    (ConfigureNamedOptions<OutboxStorageConsumerOptions>?) descriptor.ImplementationInstance;
            });

        _builder.Build(_services.Object);

        configureOptions.Should().NotBeNull();

        var options = new OutboxStorageConsumerOptions();
        configureOptions!.Action!.Invoke(options);

        options.IsolationLevel.Should().Be(_builder.IsolationLevel);
        options.BatchSize.Should().Be(_builder.BatchSize);
        options.ConsumeDelay.Should().Be(_builder.ConsumeDelay);
        options.Timeout.Should().Be(_builder.Timeout);
    }

    [Fact]
    public async Task Build_DefaultRouteIsSet_ConfiguresDefaultRoute()
    {
        _builder.OutboxStorageRegistrar = _outboxStorageRegistrar.Object;

        var isInvoked = false;
        _builder.SetDefaultRoute(builder =>
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

        IConsumePipelineRegistry? pipelineRegistry = null;
        _services.Setup(x => x.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(IConsumePipelineRegistry))))
            .Callback((ServiceDescriptor descriptor) =>
            {
                pipelineRegistry =
                    (IConsumePipelineRegistry?) descriptor.ImplementationInstance;
            });

        _builder.Build(_services.Object);

        pipelineRegistry.Should().NotBeNull();

        var defaultPipeline = pipelineRegistry.GetPipeline();

        defaultPipeline.Should().NotBeNull();

        await defaultPipeline.RunAsync(Mock.Of<IOutboxMessage>(), Mock.Of<IConsumeContext>());

        isInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task Build_RouteIsSet_ConfiguresRoute()
    {
        const string destination = "test";

        _builder.OutboxStorageRegistrar = _outboxStorageRegistrar.Object;

        var isInvoked = false;
        _builder.AddRoute(destination, builder =>
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

        IConsumePipelineRegistry? pipelineRegistry = null;
        _services.Setup(x => x.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(IConsumePipelineRegistry))))
            .Callback((ServiceDescriptor descriptor) =>
            {
                pipelineRegistry =
                    (IConsumePipelineRegistry?) descriptor.ImplementationInstance;
            });

        _builder.Build(_services.Object);

        pipelineRegistry.Should().NotBeNull();

        var pipeline = pipelineRegistry.GetPipeline(destination);

        pipeline.Should().NotBeNull();

        await pipeline.RunAsync(Mock.Of<IOutboxMessage>(), Mock.Of<IConsumeContext>());

        isInvoked.Should().BeTrue();
    }

    [Fact]
    public void Build_OutboxStorageRegistrarIsSet_RegistersConsumer()
    {
        _builder.OutboxStorageRegistrar = _outboxStorageRegistrar.Object;

        _outboxStorageRegistrar.Setup(x => x.Register(_services.Object));

        _services.Setup(x => x.Count).Returns(0);
        _services.Setup(x => x.Add(It.IsAny<ServiceDescriptor>()));

        _builder.Build(_services.Object);

        _services.Verify(x => x.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(IOutboxConsumer))),
            Times.Once);
        _services.Verify(
            x => x.Add(It.Is<ServiceDescriptor>(d =>
                d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(OutboxConsumerService))),
            Times.Once);
    }
}