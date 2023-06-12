using OutboxFlow.Configuration;

namespace OutboxFlow.Serialization;

/// <summary>
/// Extension methods for setting up serialization.
/// </summary>
public static partial class ProducePipelineStepBuilderExtensions
{
    /// <summary>
    /// Serialize a message to a JSON string, encoded as UTF-8 bytes.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TIn">Step input message type.</typeparam>
    /// <typeparam name="TOut">Step output message type.</typeparam>
    public static IProducePipelineStepBuilder<TOut, TOut> SerializeToJson<TIn, TOut>(
        this IProducePipelineStepBuilder<TIn, TOut> step)
    {
        var jsonSerializer = new JsonSerializer();

        return step.AddSyncStep((message, context) =>
        {
            context.Value = jsonSerializer.Serialize(message);
            return message;
        });
    }

    /// <summary>
    /// Serialize a message key to a JSON string, encoded as UTF-8 bytes.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <param name="keyProvider">Provides a key value to serialize.</param>
    /// <typeparam name="TIn">Step input message type.</typeparam>
    /// <typeparam name="TOut">Step output message type.</typeparam>
    /// <typeparam name="TKey">Message key type.</typeparam>
    public static IProducePipelineStepBuilder<TOut, TOut> SerializeKeyToJson<TIn, TOut, TKey>(
        this IProducePipelineStepBuilder<TIn, TOut> step, Func<TOut, TKey> keyProvider)
    {
        var jsonSerializer = new JsonSerializer();

        return step.AddSyncStep((message, context) =>
        {
            context.Key = jsonSerializer.Serialize(keyProvider(message));
            return message;
        });
    }
}