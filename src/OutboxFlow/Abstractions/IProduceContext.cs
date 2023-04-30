#pragma warning disable CA1819

using System.Data;

namespace OutboxFlow.Abstractions;

/// <summary>
/// Context for the outbox produce operation.
/// </summary>
public interface IProduceContext
{
    /// <summary>
    /// Gets or sets the destination.
    /// </summary>
    public string? Destination { get; set; }

    /// <summary>
    /// Gets or sets the message key.
    /// </summary>
    public byte[]? Key { get; set; }

    /// <summary>
    /// Gets or sets the message value.
    /// </summary>
    public byte[]? Value { get; set; }

    /// <summary>
    /// Gets the transaction.
    /// </summary>
    public IDbTransaction Transaction { get; }

    /// <summary>
    /// Gets the service provider.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the cancellation token.
    /// </summary>
    public CancellationToken CancellationToken { get; }
}

#pragma warning restore CA1819