using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutboxFlow.Storage;
using OutboxFlow.Storage.Configuration;

namespace OutboxFlow.Consume;

/// <inheritdoc />
public partial class OutboxConsumer : IOutboxConsumer
{
    private readonly IDbConnection _connection;
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
    /// <param name="connection">Database connection.</param>
    /// <param name="serviceProvider">Service provider.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">Logger.</param>
    public OutboxConsumer(IOutboxStorage outboxStorage,
        IOutboxLockManager outboxLockManager,
        IConsumePipelineRegistry registry,
        IDbConnection connection,
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
        _connection = connection;
    }

    /// <inheritdoc />
    public async ValueTask<OutboxConsumeResult> ConsumeAsync(CancellationToken cancellationToken)
    {
        using var timeoutTokenSource = new CancellationTokenSource(_options.CurrentValue.Timeout);
        using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, timeoutTokenSource.Token);

        await EnsureOpenAsync(combinedTokenSource.Token).ConfigureAwait(false);

        IOutboxLock? outboxLock;
        IReadOnlyCollection<IOutboxMessage> messages;

        using (var tx = await BeginTransactionAsync(_options.CurrentValue.IsolationLevel, combinedTokenSource.Token)
                   .ConfigureAwait(false))
        {
            try
            {
                outboxLock = await _outboxLockManager
                    .LockAsync(_options.CurrentValue.Timeout, combinedTokenSource.Token)
                    .ConfigureAwait(false);

                if (outboxLock == null)
                {
                    await tx.RollbackAsync(combinedTokenSource.Token).ConfigureAwait(false);
                    return new OutboxConsumeResult(false);
                }

                messages = await _outboxStorage
                    .FetchAsync(_options.CurrentValue.BatchSize, combinedTokenSource.Token)
                    .ConfigureAwait(false);

                await tx.CommitAsync(combinedTokenSource.Token).ConfigureAwait(false);
            }
            catch
            {
                await tx.RollbackAsync(combinedTokenSource.Token).ConfigureAwait(false);
                throw;
            }
        }

        Log.FetchedMessages(_logger, messages.Count);

        try
        {
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
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            Log.PipelineExecutionFailed(_logger, ex);
            // Use CancellationToken.None to guarantee the lock is released even if the
            // pipeline failed due to cancellation (timeout or shutdown). Passing the
            // cancelled token would skip the release, leaving the lock stuck for its TTL.
            await _outboxLockManager
                .ReleaseAsync(outboxLock, CancellationToken.None)
                .ConfigureAwait(false);
            return new OutboxConsumeResult(false);
        }

        Log.DeliveredMessages(_logger, messages.Count);

        using (var tx = await BeginTransactionAsync(_options.CurrentValue.IsolationLevel, combinedTokenSource.Token)
                   .ConfigureAwait(false))
        {
            try
            {
                await _outboxStorage
                    .DeleteAsync(messages, combinedTokenSource.Token)
                    .ConfigureAwait(false);

                await _outboxLockManager
                    .ReleaseAsync(outboxLock, combinedTokenSource.Token)
                    .ConfigureAwait(false);

                await tx.CommitAsync(combinedTokenSource.Token).ConfigureAwait(false);
            }
            catch
            {
                await tx.RollbackAsync(combinedTokenSource.Token).ConfigureAwait(false);
                throw;
            }
        }

        Log.DeletedMessages(_logger, messages.Count);

        return new OutboxConsumeResult(true, messages.Count);
    }

    private async ValueTask EnsureOpenAsync(CancellationToken ct)
    {
        if (_connection.State == ConnectionState.Closed)
            await ((DbConnection) _connection).OpenAsync(ct).ConfigureAwait(false);
    }

    internal virtual async ValueTask<DbTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel, CancellationToken ct)
    {
        var dbConnection = (DbConnection) _connection;
        return await dbConnection.BeginTransactionAsync(isolationLevel, ct).ConfigureAwait(false);
    }
}