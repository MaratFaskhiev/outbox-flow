using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using OutboxFlow.Consume;
using OutboxFlow.Storage;
using OutboxFlow.Storage.Configuration;
using Xunit;

namespace OutboxFlow.UnitTests.Consume;

public sealed class OutboxConsumerTests : IDisposable
{
    private readonly OutboxConsumer _consumer;
    private readonly OutboxStorageConsumerOptions _consumerOptions;
    private readonly Mock<IOptionsMonitor<OutboxStorageConsumerOptions>> _options;
    private readonly Mock<IOutboxLock> _outboxLock;
    private readonly Mock<IOutboxLockManager> _outboxLockManager;
    private readonly Mock<IOutboxMessage> _outboxMessage;
    private readonly Mock<IOutboxStorage> _outboxStorage;
    private readonly Mock<IPipelineStep<IConsumeContext, IOutboxMessage>> _pipeline;
    private readonly Mock<IConsumePipelineRegistry> _registry;
    private readonly Mock<IServiceProvider> _serviceProvider;

    public OutboxConsumerTests()
    {
        _options = new Mock<IOptionsMonitor<OutboxStorageConsumerOptions>>(MockBehavior.Strict);
        _outboxLockManager = new Mock<IOutboxLockManager>(MockBehavior.Strict);
        _outboxStorage = new Mock<IOutboxStorage>(MockBehavior.Strict);
        _registry = new Mock<IConsumePipelineRegistry>(MockBehavior.Strict);
        _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
        _pipeline = new Mock<IPipelineStep<IConsumeContext, IOutboxMessage>>(MockBehavior.Strict);
        _outboxLock = new Mock<IOutboxLock>(MockBehavior.Strict);
        _outboxMessage = new Mock<IOutboxMessage>(MockBehavior.Strict);

        _consumerOptions = new OutboxStorageConsumerOptions
        {
            Timeout = TimeSpan.FromMinutes(5)
        };
        _options.Setup(x => x.CurrentValue).Returns(_consumerOptions);

        _consumer = new OutboxConsumer(
            _outboxStorage.Object,
            _outboxLockManager.Object,
            _registry.Object,
            _serviceProvider.Object,
            _options.Object,
            NullLogger<OutboxConsumer>.Instance);
    }

    public void Dispose()
    {
        Mock.VerifyAll(
            _options,
            _outboxLockManager,
            _outboxStorage,
            _registry,
            _serviceProvider,
            _pipeline,
            _outboxLock);
    }

    [Fact]
    public async Task ConsumeAsync_OutboxIsAlreadyLocked_ReturnsIsNotSuccessful()
    {
        _outboxLockManager
            .Setup(x => x.LockAsync(_consumerOptions.Timeout, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IOutboxLock?) null);

        var result = await _consumer.ConsumeAsync(It.IsAny<CancellationToken>());

        result.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    public async Task ConsumeAsync_OutboxIsLocked_ReturnsIsSuccessful()
    {
        const string destination = "test";
        _outboxMessage.Setup(x => x.Destination).Returns(destination);

        _outboxLockManager
            .Setup(x => x.LockAsync(_consumerOptions.Timeout, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outboxLock.Object);

        _outboxLockManager
            .Setup(x => x.ReleaseAsync(_outboxLock.Object, It.IsAny<CancellationToken>()))
            .Returns(new ValueTask());

        _outboxStorage
            .Setup(x => x.FetchAsync(_consumerOptions.BatchSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] {_outboxMessage.Object});

        _outboxStorage.Setup(x =>
                x.DeleteAsync(
                    It.Is<IReadOnlyCollection<IOutboxMessage>>(e => e.Count == 1 && e.Contains(_outboxMessage.Object)),
                    It.IsAny<CancellationToken>()))
            .Returns(new ValueTask());

        _registry.Setup(x => x.GetPipeline(destination)).Returns(_pipeline.Object);

        IConsumeContext? context = null;
        _pipeline.Setup(x => x.RunAsync(_outboxMessage.Object, It.IsAny<IConsumeContext>()))
            .Callback((IOutboxMessage _, IConsumeContext ctx) => { context = ctx; })
            .Returns(new ValueTask());

        var result = await _consumer.ConsumeAsync(CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        context.Should().NotBeNull();
        context!.ServiceProvider.Should().BeSameAs(_serviceProvider.Object);
        context.CancellationToken.IsCancellationRequested.Should().BeFalse();
    }
}