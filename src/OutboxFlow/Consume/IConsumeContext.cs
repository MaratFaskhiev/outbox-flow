namespace OutboxFlow.Consume;

/// <summary>
/// Context for the outbox consume operation.
/// </summary>
public interface IConsumeContext
{
    /// <summary>
    /// Gets the service provider.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the cancellation token.
    /// </summary>
    public CancellationToken CancellationToken { get; }
}