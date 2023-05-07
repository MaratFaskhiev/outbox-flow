using Google.Protobuf;

namespace OutboxFlow.Serialization;

/// <summary>
/// Extension methods for setting up serialization.
/// </summary>
public static partial class ProducePipelineStepExtensions
{
    /// <summary>
    /// Serialize a message to a byte array in protobuf encoding.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TIn">Step input message type.</typeparam>
    /// <typeparam name="TOut">Step output message type.</typeparam>
    public static ProducePipelineStep<TOut, TOut> SerializeToProtobuf<TIn, TOut>(
        this ProducePipelineStep<TIn, TOut> step) where TOut : IMessage
    {
        return step.AddStep((message, context) =>
        {
            context.Value = message.ToByteArray();
            return new ValueTask<TOut>(message);
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
    public static ProducePipelineStep<TOut, TOut> SerializeKeyToProtobuf<TIn, TOut, TKey>(
        this ProducePipelineStep<TIn, TOut> step, Func<TOut, TKey> keyProvider)
        where TKey : IMessage
    {
        return step.AddStep((message, context) =>
        {
            context.Key = keyProvider(message).ToByteArray();
            return new ValueTask<TOut>(message);
        });
    }
}