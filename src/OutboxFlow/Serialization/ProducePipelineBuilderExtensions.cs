using Microsoft.Extensions.DependencyInjection;
using OutboxFlow.Configuration;

namespace OutboxFlow.Serialization;

/// <summary>
/// Extension methods for setting up serialization.
/// </summary>
public static partial class ProducePipelineBuilderExtensions
{
    /// <summary>
    /// Serialize a message by using the specified serializer.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <typeparam name="TSerializer">Serializer type.</typeparam>
    /// <typeparam name="T">Pipeline input message type.</typeparam>
    public static ProducePipelineStepBuilder<T, T> Serialize<TSerializer, T>(
        this ProducePipelineBuilder<T> pipeline)
        where TSerializer : ISerializer<byte[]>
    {
        return pipeline.AddSyncStep((message, context) =>
        {
            var serializer = context.ServiceProvider.GetRequiredService<TSerializer>();
            context.Value = serializer.Serialize(message);
            return message;
        });
    }

    /// <summary>
    /// Serialize a message key by using the specified serializer.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <param name="keyProvider">Provides a key value to serialize.</param>
    /// <typeparam name="TSerializer">Serializer type.</typeparam>
    /// <typeparam name="T">Pipeline input message type.</typeparam>
    /// <typeparam name="TKey">Message key type.</typeparam>
    public static ProducePipelineStepBuilder<T, T> SerializeKey<TSerializer, T, TKey>(
        this ProducePipelineBuilder<T> pipeline, Func<T, TKey> keyProvider)
        where TSerializer : ISerializer<byte[]>
    {
        return pipeline.AddSyncStep((message, context) =>
        {
            var serializer = context.ServiceProvider.GetRequiredService<TSerializer>();
            context.Key = serializer.Serialize(keyProvider(message));
            return message;
        });
    }
}