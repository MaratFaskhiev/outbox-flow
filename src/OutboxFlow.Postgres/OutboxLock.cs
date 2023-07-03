using OutboxFlow.Storage;

namespace OutboxFlow.Postgres;

/// <summary>
/// Represents an outbox lock.
/// </summary>
/// <param name="Id">Lock ID.</param>
/// <param name="ExpireAt">Expiration date and time.</param>
public sealed record OutboxLock(Guid Id, DateTime ExpireAt) : IOutboxLock;