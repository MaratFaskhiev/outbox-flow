using OutboxFlow.Abstractions;

namespace OutboxFlow.Postgres;

public sealed record OutboxLock(Guid Id, DateTime ExpireAt) : IOutboxLock;