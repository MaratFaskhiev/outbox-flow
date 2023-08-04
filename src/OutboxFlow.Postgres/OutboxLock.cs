using OutboxFlow.Storage;

namespace OutboxFlow.Postgres;

/// <summary>
/// Represents an outbox lock.
/// </summary>
public sealed record OutboxLock : IOutboxLock
{
    /// <summary>
    /// Represents an outbox lock.
    /// </summary>
    /// <param name="Id">Lock ID.</param>
    /// <param name="ExpireAt">Expiration date and time.</param>
    public OutboxLock(Guid Id, DateTime ExpireAt)
    {
        this.Id = Id;
        this.ExpireAt = ExpireAt;
    }

    /// <summary>Lock ID.</summary>
    public Guid Id { get; }

    /// <summary>Expiration date and time.</summary>
    public DateTime ExpireAt { get; }
}