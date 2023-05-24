#pragma warning disable CA1819

using OutboxFlow.Abstractions;

namespace OutboxFlow.Consume;

/// <inheritdoc />
public sealed class ConsumeContext : IConsumeContext
{
    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="destination">Destination.</param>
    /// <param name="key">Message key.</param>
    /// <param name="value">Message value.</param>
    /// <param name="serviceProvider">Service provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public ConsumeContext(
        string? destination,
        byte[]? key,
        byte[] value,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        Destination = destination;
        Key = key;
        Value = value;
        ServiceProvider = serviceProvider;
        CancellationToken = cancellationToken;
    }

    /// <inheritdoc />
    public string? Destination { get; set; }

    /// <inheritdoc />
    public byte[]? Key { get; set; }

    /// <inheritdoc />
    public byte[] Value { get; set; }

    /// <inheritdoc />
    public IServiceProvider ServiceProvider { get; }

    /// <inheritdoc />
    public CancellationToken CancellationToken { get; }
}

#pragma warning restore CA1819