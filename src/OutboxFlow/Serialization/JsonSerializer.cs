namespace OutboxFlow.Serialization;

/// <summary>
/// Serializes values to a JSON string, encoded as UTF-8 bytes.
/// </summary>
public sealed class JsonSerializer : ISerializer<byte[]>
{
    /// <summary>
    /// Serializes the specified value to a JSON string, encoded as UTF-8 bytes.
    /// </summary>
    /// <param name="value">Value to serialize.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TValue">Value type.</typeparam>
    public ValueTask<byte[]> SerializeAsync<TValue>(TValue value, CancellationToken cancellationToken)
    {
        var serializedValue = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value);

        return new ValueTask<byte[]>(serializedValue);
    }
}