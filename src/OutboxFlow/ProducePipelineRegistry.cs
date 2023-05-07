using OutboxFlow.Abstractions;

namespace OutboxFlow;

/// <inheritdoc />
public sealed class ProducePipelineRegistry : IProducePipelineRegistry
{
    private readonly Dictionary<Type, object> _pipelines;

    internal ProducePipelineRegistry(Dictionary<Type, object> pipelines)
    {
        _pipelines = pipelines;
    }

    /// <inheritdoc />
    public IProducePipelineStep<T> GetPipeline<T>()
    {
        if (!_pipelines.TryGetValue(typeof(T), out var pipeline))
            throw new InvalidOperationException($"Message type \"{typeof(T).FullName}\" is not registered.");

        return (IProducePipelineStep<T>)pipeline;
    }
}