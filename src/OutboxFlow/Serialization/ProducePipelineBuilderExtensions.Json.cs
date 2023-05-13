using OutboxFlow.Configuration;

namespace OutboxFlow.Serialization;

/// <summary>
/// Extension methods for setting up serialization.
/// </summary>
public static partial class ProducePipelineBuilderExtensions
{
    /// <summary>
    /// Serialize a message to a JSON string, encoded as UTF-8 bytes.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <typeparam name="T">Pipeline input message type.</typeparam>
    public static ProducePipelineStepBuilder<T, T> SerializeToJson<T>(
        this ProducePipelineBuilder<T> pipeline)
    {
        var jsonSerializer = new JsonSerializer();

        return pipeline.AddSyncStep((message, context) =>
        {
            context.Value = jsonSerializer.Serialize(message);
            return message;
        });
    }

    /// <summary>
    /// Serialize a message key to a JSON string, encoded as UTF-8 bytes.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <param name="keyProvider">Provides a key value to serialize.</param>
    /// <typeparam name="T">Pipeline input message type.</typeparam>
    /// <typeparam name="TKey">Message key type.</typeparam>
    public static ProducePipelineStepBuilder<T, T> SerializeKeyToJson<T, TKey>(
        this ProducePipelineBuilder<T> pipeline, Func<T, TKey> keyProvider)
    {
        var jsonSerializer = new JsonSerializer();

        return pipeline.AddSyncStep((message, context) =>
        {
            context.Key = jsonSerializer.Serialize(keyProvider(message));
            return message;
        });
    }
}