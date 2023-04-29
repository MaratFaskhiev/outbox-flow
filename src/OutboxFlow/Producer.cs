using OutboxFlow.Abstractions;

namespace OutboxFlow;

/// <inheritdoc />
public sealed class Producer : IProducer
{
    private readonly Dictionary<Type, object> _pipelines;

    internal Producer(Dictionary<Type, object> pipelines)
    {
        _pipelines = pipelines;
    }

    /// <inheritdoc />
    public async ValueTask ProduceAsync<T>(T message, IProduceContext context)
    {
        if (!_pipelines.TryGetValue(typeof(T), out var pipeline))
        {
            throw new InvalidOperationException($"Message type \"{typeof(T).FullName}\" is not registered.");
        }

        await ((IProducePipelineStep<T>)pipeline).InvokeAsync(message, context).ConfigureAwait(false);
    }
}