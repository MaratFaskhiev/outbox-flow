using Microsoft.Extensions.DependencyInjection;
using OutboxFlow.Configuration;

namespace OutboxFlow.Serialization;

/// <summary>
/// Extension methods for setting up serialization.
/// </summary>
public static partial class ProducePipelineStepBuilderExtensions
{
    /// <summary>
    /// Serialize a message by using the specified serializer.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TSerializer">Serializer type.</typeparam>
    /// <typeparam name="TIn">Step input message type.</typeparam>
    /// <typeparam name="TOut">Step output message type.</typeparam>
    public static IProducePipelineStepBuilder<TOut, TOut> Serialize<TSerializer, TIn, TOut>(
        this IProducePipelineStepBuilder<TIn, TOut> step)
        where TSerializer : ISerializer<byte[]>
    {
        return step.AddSyncStep((message, context) =>
        {
            var serializer = context.ServiceProvider.GetRequiredService<TSerializer>();
            context.Value = serializer.Serialize(message);
            return message;
        });
    }

    /// <summary>
    /// Serialize a message key by using the specified serializer.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <param name="keyProvider">Provides a key value to serialize.</param>
    /// <typeparam name="TSerializer">Serializer type.</typeparam>
    /// <typeparam name="TIn">Step input message type.</typeparam>
    /// <typeparam name="TOut">Step output message type.</typeparam>
    /// <typeparam name="TKey">Message key type.</typeparam>
    public static IProducePipelineStepBuilder<TOut, TOut> SerializeKey<TSerializer, TIn, TOut, TKey>(
        this IProducePipelineStepBuilder<TIn, TOut> step, Func<TOut, TKey> keyProvider)
        where TSerializer : ISerializer<byte[]>
    {
        return step.AddSyncStep((message, context) =>
        {
            var serializer = context.ServiceProvider.GetRequiredService<TSerializer>();
            context.Key = serializer.Serialize(keyProvider(message));
            return message;
        });
    }
}