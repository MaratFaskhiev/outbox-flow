using Google.Protobuf;
using OutboxFlow.Produce.Configuration;

namespace OutboxFlow.Serialization;

/// <summary>
/// Extension methods for setting up serialization.
/// </summary>
public static partial class ProducePipelineStepBuilderExtensions
{
    /// <summary>
    /// Serialize a message to a byte array in protobuf encoding.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TIn">Step input message type.</typeparam>
    /// <typeparam name="TOut">Step output message type.</typeparam>
    public static IProducePipelineStepBuilder<TOut, TOut> SerializeWithProtobuf<TIn, TOut>(
        this IProducePipelineStepBuilder<TIn, TOut> step) where TOut : IMessage
    {
        return step.AddSyncStep((message, context) =>
        {
            context.Value = message.ToByteArray();
            return message;
        });
    }

    /// <summary>
    /// Serialize a message key to a byte array in protobuf encoding.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <param name="keyProvider">Provides a key value to serialize.</param>
    /// <typeparam name="TIn">Step input message type.</typeparam>
    /// <typeparam name="TOut">Step output message type.</typeparam>
    /// <typeparam name="TKey">Message key type.</typeparam>
    public static IProducePipelineStepBuilder<TOut, TOut> SerializeKeyWithProtobuf<TIn, TOut, TKey>(
        this IProducePipelineStepBuilder<TIn, TOut> step, Func<TOut, TKey> keyProvider)
        where TKey : IMessage
    {
        return step.AddSyncStep((message, context) =>
        {
            context.Key = keyProvider(message).ToByteArray();
            return message;
        });
    }
}