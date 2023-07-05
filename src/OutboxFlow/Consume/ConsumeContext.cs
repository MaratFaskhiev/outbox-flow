namespace OutboxFlow.Consume;

/// <inheritdoc />
public sealed class ConsumeContext : IConsumeContext
{
    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public ConsumeContext(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        ServiceProvider = serviceProvider;
        CancellationToken = cancellationToken;
    }

    /// <inheritdoc />
    public IServiceProvider ServiceProvider { get; }

    /// <inheritdoc />
    public CancellationToken CancellationToken { get; }
}