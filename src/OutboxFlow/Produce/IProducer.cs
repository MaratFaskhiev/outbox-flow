using System.Data;

namespace OutboxFlow.Produce;

/// <summary>
/// Produces an outbox message.
/// </summary>
public interface IProducer
{
    /// <summary>
    /// Produces an outbox message.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="transaction">Transaction.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="T">Message type.</typeparam>
    ValueTask ProduceAsync<T>(T message, IDbTransaction transaction, CancellationToken cancellationToken);
}