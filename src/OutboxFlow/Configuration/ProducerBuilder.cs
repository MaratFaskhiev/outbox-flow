using OutboxFlow.Abstractions;
using OutboxFlow.Produce;

namespace OutboxFlow.Configuration;

/// <summary>
/// Builds an outbox producer.
/// </summary>
public sealed class ProducerBuilder
{
    private readonly Dictionary<Type, object> _messagePipelines = new();

    /// <summary>
    /// Configures produce pipeline for the specified message type.
    /// </summary>
    /// <param name="configure">Configure action.</param>
    /// <typeparam name="T">Message type.</typeparam>
    public ProducerBuilder ForMessage<T>(Action<ProducePipelineBuilder<T>> configure)
    {
        if (_messagePipelines.ContainsKey(typeof(T)))
            throw new InvalidOperationException(
                $"Produce pipeline for the message type \"{typeof(T).Name}\" is already registered.");

        var pipelineBuilder = new ProducePipelineBuilder<T>();
        configure(pipelineBuilder);
        _messagePipelines.Add(typeof(T), pipelineBuilder.Build());

        return this;
    }

    /// <summary>
    /// Builds a pipeline registry.
    /// </summary>
    public IProducePipelineRegistry BuildRegistry()
    {
        return new ProducePipelineRegistry(_messagePipelines);
    }
}