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
    /// <typeparam name="TValue">Value type.</typeparam>
    T Serialize<TValue>(TValue value);
}