namespace OutboxFlow.Abstractions;

/// <summary>
/// Outbox lock.
/// </summary>
public interface IOutboxLock
{
    /// <summary>
    /// Gets the lock ID.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the lock expiration date and time.
    /// </summary>
    public DateTime ExpireAt { get; }
}