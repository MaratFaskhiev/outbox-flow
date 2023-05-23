using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutboxFlow.Abstractions;
using OutboxFlow.Configuration;

namespace OutboxFlow.Consume;

/// <summary>
/// Consumes stored outbox messages.
/// </summary>
public sealed class OutboxStorageConsumer : BackgroundService
{
    private readonly ILogger<OutboxStorageConsumer> _logger;
    private readonly IOptionsMonitor<OutboxStorageConsumerOptions> _options;
    private readonly IOutboxStorage _outboxStorage;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="outboxStorage">Outbox storage.</param>
    /// <param name="serviceScopeFactory">Service scope factory.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">Logger.</param>
    public OutboxStorageConsumer(
        IOutboxStorage outboxStorage,
        IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<OutboxStorageConsumerOptions> options,
        ILogger<OutboxStorageConsumer> logger)
    {
        _outboxStorage = outboxStorage;
        _options = options;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
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
                var consumedCount = 0;
                try
                {
                    consumedCount = await ConsumeMessagesAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    _logger.LogError(e, "Failed to process outbox messages.");
                }

                if (consumedCount != _options.CurrentValue.BatchSize)
                    await Task.Delay(_options.CurrentValue.ConsumeDelay, stoppingToken).ConfigureAwait(false);
            }
        }
        finally
        {
            _logger.LogInformation("Service is stopped.");
        }
    }

    private async ValueTask<int> ConsumeMessagesAsync(CancellationToken stoppingToken)
    {
        using var serviceScope = _serviceScopeFactory.CreateScope();

        var connectionFactory = serviceScope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();

        using var connection = connectionFactory.Create();
        connection.Open();

        // TODO: add lock capabilities.

        IReadOnlyCollection<IOutboxMessage> messages;
        using (var transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead))
        {
            messages = await _outboxStorage.FetchAsync(
                _options.CurrentValue.BatchSize,
                transaction,
                stoppingToken
            ).ConfigureAwait(false);

            transaction.Commit();
        }

        _logger.LogDebug("Fetched {Count} messages.", messages.Count);

        // Deliver messages.
        await Task.Delay(1, stoppingToken).ConfigureAwait(false);

        _logger.LogDebug("Delivered {Count} messages.", messages.Count);

        using (var transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead))
        {
            await _outboxStorage.DeleteAsync(
                messages,
                transaction,
                stoppingToken
            ).ConfigureAwait(false);

            transaction.Commit();
        }

        _logger.LogDebug("Deleted {Count} messages.", messages.Count);

        return messages.Count;
    }
}