using System.Data;

namespace OutboxFlow.Produce;

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
    public string? Destination { get; set; }

    /// <inheritdoc />
    public IDbTransaction Transaction { get; }

    /// <inheritdoc />
    public IServiceProvider ServiceProvider { get; }

    /// <inheritdoc />
    public CancellationToken CancellationToken { get; }

#pragma warning disable CA1819
    /// <inheritdoc />
    public byte[]? Key { get; set; }

    /// <inheritdoc />
    public byte[]? Value { get; set; }
#pragma warning restore CA1819
}