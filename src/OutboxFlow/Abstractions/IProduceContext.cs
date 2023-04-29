using System.Data;

namespace OutboxFlow.Abstractions;

/// <summary>
/// Context for the outbox produce operation.
/// </summary>
public interface IProduceContext
{
    /// <summary>
    /// Gets the transaction.
    /// </summary>
    public IDbTransaction Transaction { get; }

    /// <summary>
    /// Gets the cancellation token.
    /// </summary>
    public CancellationToken CancellationToken { get; }
}