using System.Data;
using OutboxFlow.Produce;

namespace OutboxFlow.Storage;

/// <summary>
/// Outbox message storage.
/// </summary>
public interface IOutboxStorage
{
    /// <summary>
    /// Fetches outbox messages from the storage.
    /// </summary>
    /// <param name="batchSize">Amount of messages to fetch.</param>
    /// <param name="transaction">Transaction.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask<IReadOnlyCollection<IOutboxMessage>> FetchAsync(
        int batchSize,
        IDbTransaction transaction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves an outbox message to the storage.
    /// </summary>
    /// <param name="context">Produce context.</param>
    ValueTask SaveAsync(IProduceContext context);

    /// <summary>
    /// Saves multiple outbox messages to the storage in a single operation.
    /// </summary>
    /// <param name="contexts">Produce contexts.</param>
    ValueTask SaveBatchAsync(IReadOnlyCollection<IProduceContext> contexts);

    /// <summary>
    /// Deletes outbox messages from the storage.
    /// </summary>
    /// <param name="outboxMessages">Outbox messages.</param>
    /// <param name="transaction">Transaction.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask DeleteAsync(
        IReadOnlyCollection<IOutboxMessage> outboxMessages,
        IDbTransaction transaction,
        CancellationToken cancellationToken = default);
}