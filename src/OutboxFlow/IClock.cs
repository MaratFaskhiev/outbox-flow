namespace OutboxFlow;

/// <summary>
/// Provides date and time functions.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current date and time.
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Creates a <see cref="Task" /> that will complete after a time delay.
    /// </summary>
    /// <param name="delay">The time span to wait before completing the returned Task</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task Delay(TimeSpan delay, CancellationToken cancellationToken);
}