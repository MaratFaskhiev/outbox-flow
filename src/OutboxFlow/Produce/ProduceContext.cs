namespace OutboxFlow.Produce;

/// <inheritdoc />
public sealed class ProduceContext : IProduceContext
{
    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public ProduceContext(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken,
        IDictionary<string, string>? headers = null)
    {
        ServiceProvider = serviceProvider;
        CancellationToken = cancellationToken;

        Headers = headers != null
            ? new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public string? Destination { get; set; }

    /// <inheritdoc />
    public IServiceProvider ServiceProvider { get; }

    /// <inheritdoc />
    public CancellationToken CancellationToken { get; }

    /// <inheritdoc />
    public IDictionary<string, string> Headers { get; }

#pragma warning disable CA1819
    /// <inheritdoc />
    public byte[]? Key { get; set; }

    /// <inheritdoc />
    public byte[]? Value { get; set; }
#pragma warning restore CA1819
}