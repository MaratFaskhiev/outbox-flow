using OutboxFlow.Abstractions;

namespace OutboxFlow.Produce;

/// <summary>
/// Outbox produce pipeline.
/// </summary>
/// <typeparam name="T">Message type to produce.</typeparam>
public sealed class ProducePipeline<T> : IProducePipelineStep<T>
{
    private readonly IProducePipelineStep<T>? _step;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="step">First step.</param>
    public ProducePipeline(IProducePipelineStep<T>? step)
    {
        _step = step;
    }

    /// <inheritdoc />
    public async ValueTask InvokeAsync(T message, IProduceContext context)
    {
        if (_step == null) return;

        await _step.InvokeAsync(message, context).ConfigureAwait(false);
    }
}