namespace OutboxFlow.Serialization;

public static partial class ProducePipelineStepExtensions
{
    /// <summary>
    /// Serialize a message to a JSON string, encoded as UTF-8 bytes.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TIn">Step input message type.</typeparam>
    /// <typeparam name="TOut">Step output message type.</typeparam>
    public static ProducePipelineStep<TOut, TOut> SerializeToJson<TIn, TOut>(
        this ProducePipelineStep<TIn, TOut> step)
    {
        var jsonSerializer = new JsonSerializer();

        return step.AddStep(async (message, context) =>
        {
            context.Value = await jsonSerializer.SerializeAsync(message, context.CancellationToken)
                .ConfigureAwait(false);
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
    public static ProducePipelineStep<TOut, TOut> SerializeKeyToJson<TIn, TOut, TKey>(
        this ProducePipelineStep<TIn, TOut> step, Func<TOut, TKey> keyProvider)
    {
        var jsonSerializer = new JsonSerializer();

        return step.AddStep(async (message, context) =>
        {
            context.Key = await jsonSerializer.SerializeAsync(keyProvider(message), context.CancellationToken)
                .ConfigureAwait(false);
            return message;
        });
    }
}