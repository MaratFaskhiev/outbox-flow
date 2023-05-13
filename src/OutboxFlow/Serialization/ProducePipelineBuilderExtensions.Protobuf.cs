using Google.Protobuf;
using OutboxFlow.Configuration;

namespace OutboxFlow.Serialization;

/// <summary>
/// Extension methods for setting up serialization.
/// </summary>
public static class ProducePipelineExtensions
{
    /// <summary>
    /// Serialize a message to a byte array in protobuf encoding.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <typeparam name="T">Pipeline input message type.</typeparam>
    public static ProducePipelineStepBuilder<T, T> SerializeToProtobuf<T>(
        this ProducePipelineBuilder<T> pipeline) where T : IMessage
    {
        return pipeline.AddSyncStep((message, context) =>
        {
            context.Value = message.ToByteArray();
            return message;
        });
    }

    /// <summary>
    /// Serialize a message key to a byte array in protobuf encoding.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <param name="keyProvider">Provides a key value to serialize.</param>
    /// <typeparam name="T">Pipeline input message type.</typeparam>
    /// <typeparam name="TKey">Message key type.</typeparam>
    public static ProducePipelineStepBuilder<T, T> SerializeKeyToProtobuf<T, TKey>(
        this ProducePipelineBuilder<T> pipeline, Func<T, TKey> keyProvider)
        where TKey : IMessage
    {
        return pipeline.AddSyncStep((message, context) =>
        {
            context.Key = keyProvider(message).ToByteArray();
            return message;
        });
    }
}