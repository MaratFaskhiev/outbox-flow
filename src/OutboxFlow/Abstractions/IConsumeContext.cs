#pragma warning disable CA1819

namespace OutboxFlow.Abstractions;

/// <summary>
/// Context for the outbox consume operation.
/// </summary>
public interface IConsumeContext
{
    /// <summary>
    /// Gets the destination.
    /// </summary>
    public string Destination { get; }

    /// <summary>
    /// Gets the message key.
    /// </summary>
    public byte[]? Key { get; }

    /// <summary>
    /// Gets the message value.
    /// </summary>
    public byte[] Value { get; }

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