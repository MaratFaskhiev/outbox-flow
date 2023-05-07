namespace OutboxFlow.Abstractions;

/// <summary>
/// Outbox message storage.
/// </summary>
public interface IStorage
{
    /// <summary>
    /// Saves an outbox message to the storage.
    /// </summary>
    /// <param name="context">Produce context.</param>
    ValueTask SaveAsync(IProduceContext context);
}