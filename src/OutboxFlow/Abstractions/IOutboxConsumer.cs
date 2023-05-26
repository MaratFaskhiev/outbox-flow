namespace OutboxFlow.Abstractions;

/// <summary>
/// Consumes stored outbox messages.
/// </summary>
public interface IOutboxConsumer
{
    /// <summary>
    /// Consumes stored outbox messages.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask<OutboxConsumeResult> ConsumeAsync(CancellationToken cancellationToken);
}