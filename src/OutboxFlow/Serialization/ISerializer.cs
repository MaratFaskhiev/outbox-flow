namespace OutboxFlow.Serialization;

/// <summary>
/// Serializes values.
/// </summary>
/// <typeparam name="T">Target type to serialize.</typeparam>
public interface ISerializer<T>
{
    /// <summary>
    /// Serializes the specified value.
    /// </summary>
    /// <param name="value">Value to serialize.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TValue">Value type.</typeparam>
    ValueTask<T> SerializeAsync<TValue>(TValue value, CancellationToken cancellationToken);
}