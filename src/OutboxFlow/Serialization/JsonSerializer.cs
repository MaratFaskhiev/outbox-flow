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
    /// <typeparam name="TValue">Value type.</typeparam>
    public byte[] Serialize<TValue>(TValue value)
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value);
    }
}