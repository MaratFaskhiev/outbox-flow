using OutboxFlow.Abstractions;

namespace OutboxFlow;

/// <summary>
/// Outbox produce pipeline.
/// </summary>
/// <typeparam name="T">Message type to produce.</typeparam>
public sealed class ProducePipeline<T> : IProducePipelineStep<T>
{
    private IProducePipelineStep<T>? _step;

    /// <inheritdoc />
    public async ValueTask InvokeAsync(T message, IProduceContext context)
    {
        if (_step == null) return;

        await _step.InvokeAsync(message, context).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds the first step to the pipeline.
    /// </summary>
    /// <param name="action">Step.</param>
    /// <typeparam name="TOut">Output parameter type.</typeparam>
    public ProducePipelineStep<T, TOut> AddStep<TOut>(
        Func<T, IProduceContext, ValueTask<TOut>> action)
    {
        if (_step != null) throw new InvalidOperationException("The first step is already added.");

        var step = new ProducePipelineStep<T, TOut>(action);
        _step = step;
        return step;
    }
}