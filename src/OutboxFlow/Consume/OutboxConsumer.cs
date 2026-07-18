using System.Transactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutboxFlow.Storage;
using OutboxFlow.Storage.Configuration;

namespace OutboxFlow.Consume;

/// <inheritdoc />
public sealed partial class OutboxConsumer : IOutboxConsumer
{
    private readonly ILogger<OutboxConsumer> _logger;
    private readonly IOptionsMonitor<OutboxStorageConsumerOptions> _options;
    private readonly IOutboxLockManager _outboxLockManager;
    private readonly IOutboxStorage _outboxStorage;
    private readonly IConsumePipelineRegistry _registry;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="outboxStorage">Outbox storage.</param>
    /// <param name="outboxLockManager">Outbox state storage.</param>
    /// <param name="registry">Consume pipeline registry.</param>
    /// <param name="serviceProvider">Service provider.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">Logger.</param>
    public OutboxConsumer(
        IOutboxStorage outboxStorage,
        IOutboxLockManager outboxLockManager,
        IConsumePipelineRegistry registry,
        IServiceProvider serviceProvider,
        IOptionsMonitor<OutboxStorageConsumerOptions> options,
        ILogger<OutboxConsumer> logger)
    {
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

        IOutboxLock? outboxLock;
        IReadOnlyCollection<IOutboxMessage> messages;
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            outboxLock = await _outboxLockManager
                .LockAsync(_options.CurrentValue.Timeout, combinedTokenSource.Token)
                .ConfigureAwait(false);

            if (outboxLock == null) return new OutboxConsumeResult(false);

            messages = await _outboxStorage
                .FetchAsync(_options.CurrentValue.BatchSize, combinedTokenSource.Token)
                .ConfigureAwait(false);

            scope.Complete();
        }

        Log.FetchedMessages(_logger, messages.Count);

        foreach (var message in messages)
        {
            var context = new ConsumeContext(
                _serviceProvider,
                combinedTokenSource.Token);

            var consumePipeline = message.Destination != null
                ? _registry.GetPipeline(message.Destination)
                : _registry.GetPipeline();
            await consumePipeline.RunAsync(message, context).ConfigureAwait(false);
        }

        Log.DeliveredMessages(_logger, messages.Count);

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await _outboxStorage
                .DeleteAsync(messages, combinedTokenSource.Token
                ).ConfigureAwait(false);

            await _outboxLockManager
                .ReleaseAsync(outboxLock, combinedTokenSource.Token)
                .ConfigureAwait(false);

            scope.Complete();
        }

        Log.DeletedMessages(_logger, messages.Count);

        return new OutboxConsumeResult(true, messages.Count);
    }
}