namespace OutboxFlow.Abstractions;

/// <summary>
/// Produces an outbox message.
/// </summary>
public interface IProducer
{
    /// <summary>
    /// Produces an outbox message.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="context">Produce context.</param>
    ValueTask ProduceAsync<T>(T message, IProduceContext context);
}