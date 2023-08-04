using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using OutboxFlow.Consume;
using OutboxFlow.Storage.Configuration;
using Xunit;

namespace OutboxFlow.UnitTests.Consume;

public sealed class OutboxConsumerServiceTests : IDisposable
{
    private readonly Mock<IClock> _clock;
    private readonly OutboxStorageConsumerOptions _consumerOptions;
    private readonly Mock<IOptionsMonitor<OutboxStorageConsumerOptions>> _options;
    private readonly Mock<IOutboxConsumer> _outboxConsumer;
    private readonly OutboxConsumerService _service;
    private readonly Mock<IServiceProvider> _serviceProvider;
    private readonly Mock<IServiceScope> _serviceScope;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;

    public OutboxConsumerServiceTests()
    {
        _serviceScopeFactory = new Mock<IServiceScopeFactory>(MockBehavior.Strict);
        _serviceScope = new Mock<IServiceScope>(MockBehavior.Strict);
        _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
        _outboxConsumer = new Mock<IOutboxConsumer>(MockBehavior.Strict);
        _options = new Mock<IOptionsMonitor<OutboxStorageConsumerOptions>>(MockBehavior.Strict);
        _clock = new Mock<IClock>(MockBehavior.Strict);

        _serviceScopeFactory.Setup(x => x.CreateScope()).Returns(_serviceScope.Object);
        _serviceScope.Setup(x => x.ServiceProvider).Returns(_serviceProvider.Object);
        _serviceScope.Setup(x => x.Dispose());
        _serviceProvider.Setup(x => x.GetService(typeof(IOutboxConsumer))).Returns(_outboxConsumer.Object);

        _consumerOptions = new OutboxStorageConsumerOptions();
        _options.Setup(x => x.CurrentValue).Returns(_consumerOptions);

        _service = new OutboxConsumerService(
            _serviceScopeFactory.Object,
            _options.Object,
            _clock.Object,
            NullLogger<OutboxConsumerService>.Instance);
    }

    public void Dispose()
    {
        _service.Dispose();

        Mock.VerifyAll(
            _serviceScopeFactory,
            _serviceScope,
            _serviceProvider,
            _outboxConsumer,
            _options,
            _clock);
    }

    [Fact]
    public async Task ExecuteAsync_ConsumeIsNotSuccessful_WaitsTimeout()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        _clock.Setup(x => x.Delay(_consumerOptions.Timeout, It.IsAny<CancellationToken>()))
            .Callback(() => cts.Cancel())
            .Returns(Task.CompletedTask);

        _outboxConsumer.Setup(x => x.ConsumeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OutboxConsumeResult(false));

        await _service.StartAsync(cts.Token);

        while (!cts.IsCancellationRequested)
        {
        }

        _clock.Verify(x => x.Delay(_consumerOptions.Timeout, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ConsumedCountEqualsToBatchSize_NoWait()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        var invocationCount = 0;
        _outboxConsumer.Setup(x => x.ConsumeAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                if (invocationCount > 0) cts.Cancel();

                invocationCount++;
            })
            .ReturnsAsync(new OutboxConsumeResult(true, _consumerOptions.BatchSize));

        await _service.StartAsync(cts.Token);

        while (!cts.IsCancellationRequested)
        {
        }

        _clock.Verify(x => x.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ConsumedCountLessThanBatchSize_WaitsConsumeDelay()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        _clock.Setup(x => x.Delay(_consumerOptions.ConsumeDelay, It.IsAny<CancellationToken>()))
            .Callback(() => cts.Cancel())
            .Returns(Task.CompletedTask);

        _outboxConsumer.Setup(x => x.ConsumeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OutboxConsumeResult(true, _consumerOptions.BatchSize - 1));

        await _service.StartAsync(cts.Token);

        while (!cts.IsCancellationRequested)
        {
        }

        _clock.Verify(x => x.Delay(_consumerOptions.ConsumeDelay, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ConsumeThrownException_WaitsTimeout()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        _clock.Setup(x => x.Delay(_consumerOptions.Timeout, It.IsAny<CancellationToken>()))
            .Callback(() => cts.Cancel())
            .Returns(Task.CompletedTask);

        _outboxConsumer.Setup(x => x.ConsumeAsync(It.IsAny<CancellationToken>())).Throws<Exception>();

        await _service.StartAsync(cts.Token);

        while (!cts.IsCancellationRequested)
        {
        }

        _clock.Verify(x => x.Delay(_consumerOptions.Timeout, It.IsAny<CancellationToken>()), Times.Once);
    }
}