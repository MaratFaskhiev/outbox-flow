using Microsoft.Extensions.DependencyInjection;

namespace OutboxFlow.Serialization;

/// <summary>
/// Extension methods for setting up serialization.
/// </summary>
public static partial class ProducePipelineExtensions
{
    /// <summary>
    /// Serialize a message by using the specified serializer.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <typeparam name="TSerializer">Serializer type.</typeparam>
    /// <typeparam name="T">Pipeline input message type.</typeparam>
    public static ProducePipelineStep<T, T> Serialize<TSerializer, T>(
        this ProducePipeline<T> pipeline)
        where TSerializer : ISerializer<byte[]>
    {
        return pipeline.AddStep(async (message, context) =>
        {
            var serializer = context.ServiceProvider.GetRequiredService<TSerializer>();
            context.Value = await serializer.SerializeAsync(message, context.CancellationToken)
                .ConfigureAwait(false);
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
    public static ProducePipelineStep<T, T> SerializeKey<TSerializer, T, TKey>(
        this ProducePipeline<T> pipeline, Func<T, TKey> keyProvider)
        where TSerializer : ISerializer<byte[]>
    {
        return pipeline.AddStep(async (message, context) =>
        {
            var serializer = context.ServiceProvider.GetRequiredService<TSerializer>();
            context.Key = await serializer.SerializeAsync(keyProvider(message), context.CancellationToken)
                .ConfigureAwait(false);
            return message;
        });
    }
}