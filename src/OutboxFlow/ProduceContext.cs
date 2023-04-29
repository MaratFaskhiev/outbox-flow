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
    /// <param name="serviceProvider">Service provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public ProduceContext(
        IDbTransaction transaction, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Transaction = transaction;
        ServiceProvider = serviceProvider;
        CancellationToken = cancellationToken;
    }

    /// <inheritdoc />
    public IDbTransaction Transaction { get; }

    /// <inheritdoc />
    public IServiceProvider ServiceProvider { get; }

    /// <inheritdoc />
    public CancellationToken CancellationToken { get; }
}