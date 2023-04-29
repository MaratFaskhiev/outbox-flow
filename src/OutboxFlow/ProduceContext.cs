using System.Data;
using OutboxFlow.Abstractions;

namespace OutboxFlow;

/// <inheritdoc />
public sealed class ProduceContext : IProduceContext
{
    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="transaction">Transaction.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public ProduceContext(IDbTransaction transaction, CancellationToken cancellationToken)
    {
        Transaction = transaction;
        CancellationToken = cancellationToken;
    }

    /// <inheritdoc />
    public IDbTransaction Transaction { get; }

    /// <inheritdoc />
    public CancellationToken CancellationToken { get; }
}