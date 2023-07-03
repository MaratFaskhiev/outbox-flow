using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutboxFlow.Storage;
using OutboxFlow.Storage.Configuration;

namespace OutboxFlow.Consume;

/// <inheritdoc />
public sealed class OutboxConsumer : IOutboxConsumer
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<OutboxConsumer> _logger;
    private readonly IOptionsMonitor<OutboxStorageConsumerOptions> _options;
    private readonly IOutboxLockManager _outboxLockManager;
    private readonly IOutboxStorage _outboxStorage;
    private readonly IConsumePipelineRegistry _registry;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="connectionFactory">Database connection factory.</param>
    /// <param name="outboxStorage">Outbox storage.</param>
    /// <param name="outboxLockManager">Outbox state storage.</param>
    /// <param name="registry">Consume pipeline registry.</param>
    /// <param name="serviceProvider">Service provider.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">Logger.</param>
    public OutboxConsumer(
        IDbConnectionFactory connectionFactory,
        IOutboxStorage outboxStorage,
        IOutboxLockManager outboxLockManager,
        IConsumePipelineRegistry registry,
        IServiceProvider serviceProvider,
        IOptionsMonitor<OutboxStorageConsumerOptions> options,
        ILogger<OutboxConsumer> logger)
    {
        _connectionFactory = connectionFactory;
        _outboxStorage = outboxStorage;
        _outboxLockManager = outboxLockManager;
        _registry = registry;
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask<OutboxConsumeResult> ConsumeAsync(CancellationToken cancellationToken)
    {
        using var timeoutTokenSource = new CancellationTokenSource(_options.CurrentValue.Timeout);
        using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, timeoutTokenSource.Token);

        using var connection = _connectionFactory.Create();
        connection.Open();

        IOutboxLock? outboxLock;
        IReadOnlyCollection<IOutboxMessage> messages;
        using (var transaction = connection.BeginTransaction(_options.CurrentValue.IsolationLevel))
        {
            outboxLock = await _outboxLockManager
                .LockAsync(_options.CurrentValue.Timeout, transaction, combinedTokenSource.Token)
                .ConfigureAwait(false);

            if (outboxLock == null) return new OutboxConsumeResult(false);

            messages = await _outboxStorage
                .FetchAsync(_options.CurrentValue.BatchSize, transaction, combinedTokenSource.Token)
                .ConfigureAwait(false);

            transaction.Commit();
        }

        _logger.LogDebug("Fetched {Count} messages.", messages.Count);

        foreach (var message in messages)
        {
            var context = new ConsumeContext(
                message.Destination,
                message.Key,
                message.Value,
                _serviceProvider,
                combinedTokenSource.Token);

            var consumePipeline = _registry.GetPipeline(message.Destination);
            await consumePipeline.InvokeAsync(message, context).ConfigureAwait(false);
        }

        _logger.LogDebug("Delivered {Count} messages.", messages.Count);

        using (var transaction = connection.BeginTransaction(_options.CurrentValue.IsolationLevel))
        {
            await _outboxStorage
                .DeleteAsync(messages, transaction, combinedTokenSource.Token
                ).ConfigureAwait(false);

            await _outboxLockManager
                .ReleaseAsync(outboxLock, transaction, combinedTokenSource.Token)
                .ConfigureAwait(false);

            transaction.Commit();
        }

        _logger.LogDebug("Deleted {Count} messages.", messages.Count);

        return new OutboxConsumeResult(true, messages.Count);
    }
}