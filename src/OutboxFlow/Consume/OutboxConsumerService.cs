using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutboxFlow.Storage.Configuration;

namespace OutboxFlow.Consume;

/// <summary>
/// Background service which consumes stored outbox messages.
/// </summary>
public sealed class OutboxConsumerService : BackgroundService
{
    private readonly IClock _clock;
    private readonly ILogger<OutboxConsumerService> _logger;
    private readonly IOptionsMonitor<OutboxStorageConsumerOptions> _options;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="serviceScopeFactory">Service scope factory.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="clock">Clock.</param>
    /// <param name="logger">Logger.</param>
    public OutboxConsumerService(
        IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<OutboxStorageConsumerOptions> options,
        IClock clock,
        ILogger<OutboxConsumerService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _options = options;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Service is started.");

        await Task.Yield();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                OutboxConsumeResult? consumeResult = null;
                try
                {
                    using var serviceScope = _serviceScopeFactory.CreateScope();
                    var consumer = serviceScope.ServiceProvider.GetRequiredService<IOutboxConsumer>();

                    consumeResult = await consumer.ConsumeAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    _logger.LogError(e, "Failed to process outbox messages.");
                }

                if (!(consumeResult?.IsSuccessful ?? false))
                    await _clock.Delay(_options.CurrentValue.Timeout, stoppingToken).ConfigureAwait(false);
                else if (consumeResult.Count != _options.CurrentValue.BatchSize)
                    await _clock.Delay(_options.CurrentValue.ConsumeDelay, stoppingToken).ConfigureAwait(false);
            }
        }
        finally
        {
            _logger.LogInformation("Service is stopped.");
        }
    }
}