using OutboxFlow.Abstractions;
using OutboxFlow.Pipelines;

namespace OutboxFlow.Configuration;

/// <inheritdoc />
public sealed class ProducePipelineBuilder<T> : IProducePipelineBuilder<T>
{
    private IPipelineStepBuilder<IProduceContext, T>? _step;

    /// <inheritdoc />
    public IPipelineStep<IProduceContext, T> Build()
    {
        return new Pipeline<IProduceContext, T>(_step?.Build());
    }

    /// <inheritdoc />
    public IProducePipelineStepBuilder<T, TOut> AddStep<TOut>(
        Func<T, IProduceContext, ValueTask<TOut>> action)
    {
        if (_step != null) throw new InvalidOperationException("The first step is already added.");

        var step = new ProducePipelineStepBuilder<T, TOut>(action);
        _step = step;
        return step;
    }

    /// <inheritdoc />
    public IProducePipelineStepBuilder<T, TOut> AddSyncStep<TOut>(
        Func<T, IProduceContext, TOut> action)
    {
        if (_step != null) throw new InvalidOperationException("The first step is already added.");

        var step = new ProducePipelineStepBuilder<T, TOut>(action);
        _step = step;
        return step;
    }
}