using OutboxFlow.Abstractions;
using OutboxFlow.Produce;

namespace OutboxFlow.Configuration;

/// <summary>
/// Outbox produce pipeline builder.
/// </summary>
/// <typeparam name="T">Message type to produce.</typeparam>
public sealed class ProducePipelineBuilder<T> : IProducePipelineStepBuilder<T>
{
    private IProducePipelineStepBuilder<T>? _step;

    /// <inheritdoc />
    public IProducePipelineStep<T> Build()
    {
        return new ProducePipeline<T>(_step?.Build());
    }

    /// <summary>
    /// Adds the first step to the pipeline.
    /// </summary>
    /// <param name="action">Step.</param>
    /// <typeparam name="TOut">Output parameter type.</typeparam>
    public ProducePipelineStepBuilder<T, TOut> AddStep<TOut>(
        Func<T, IProduceContext, ValueTask<TOut>> action)
    {
        if (_step != null) throw new InvalidOperationException("The first step is already added.");

        var step = new ProducePipelineStepBuilder<T, TOut>(action);
        _step = step;
        return step;
    }

    /// <summary>
    /// Adds the first step to the pipeline.
    /// </summary>
    /// <param name="action">Step.</param>
    /// <typeparam name="TOut">Output parameter type.</typeparam>
    public ProducePipelineStepBuilder<T, TOut> AddSyncStep<TOut>(
        Func<T, IProduceContext, TOut> action)
    {
        if (_step != null) throw new InvalidOperationException("The first step is already added.");

        var step = new ProducePipelineStepBuilder<T, TOut>(action);
        _step = step;
        return step;
    }
}