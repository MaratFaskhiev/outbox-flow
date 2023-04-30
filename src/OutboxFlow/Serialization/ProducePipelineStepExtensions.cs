using Microsoft.Extensions.DependencyInjection;

namespace OutboxFlow.Serialization;

public static partial class ProducePipelineStepExtensions
{
    /// <summary>
    /// Serialize a message by using the specified serializer.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TSerializer">Serializer type.</typeparam>
    /// <typeparam name="TIn">Step input message type.</typeparam>
    /// <typeparam name="TOut">Step output message type.</typeparam>
    public static ProducePipelineStep<TOut, TOut> Serialize<TSerializer, TIn, TOut>(
        this ProducePipelineStep<TIn, TOut> step)
        where TSerializer : ISerializer<byte[]>
    {
        return step.AddStep(async (message, context) =>
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
    /// <param name="step">Step.</param>
    /// <param name="keyProvider">Provides a key value to serialize.</param>
    /// <typeparam name="TSerializer">Serializer type.</typeparam>
    /// <typeparam name="TIn">Step input message type.</typeparam>
    /// <typeparam name="TOut">Step output message type.</typeparam>
    /// <typeparam name="TKey">Message key type.</typeparam>
    public static ProducePipelineStep<TOut, TOut> SerializeKey<TSerializer, TIn, TOut, TKey>(
        this ProducePipelineStep<TIn, TOut> step, Func<TOut, TKey> keyProvider)
        where TSerializer : ISerializer<byte[]>
    {
        return step.AddStep(async (message, context) =>
        {
            var serializer = context.ServiceProvider.GetRequiredService<TSerializer>();
            context.Key = await serializer.SerializeAsync(keyProvider(message), context.CancellationToken)
                .ConfigureAwait(false);
            return message;
        });
    }
}