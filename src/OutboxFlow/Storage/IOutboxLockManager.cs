using System.Data;

namespace OutboxFlow.Storage;

/// <summary>
/// Outbox lock manager.
/// </summary>
public interface IOutboxLockManager
{
    /// <summary>
    /// Locks the outbox.
    /// </summary>
    /// <param name="lockTimeout">Lock timeout.</param>
    /// <param name="transaction">Transaction.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="IOutboxLock" /> instance if lock was successfully acquired, otherwise <c>null</c>.</returns>
    ValueTask<IOutboxLock?> LockAsync(
        TimeSpan lockTimeout, IDbTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases the outbox lock.
    /// </summary>
    /// <param name="outboxLock">Outbox lock.</param>
    /// <param name="transaction">Transaction.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask ReleaseAsync(
        IOutboxLock outboxLock,
        IDbTransaction transaction,
        CancellationToken cancellationToken = default);
}