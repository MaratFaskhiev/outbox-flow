using System.Data;

namespace OutboxFlow.Abstractions;

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