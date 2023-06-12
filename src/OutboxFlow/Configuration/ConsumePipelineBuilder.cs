using OutboxFlow.Abstractions;
using OutboxFlow.Pipelines;

namespace OutboxFlow.Configuration;

/// <inheritdoc />
public sealed class ConsumePipelineBuilder : IConsumePipelineBuilder
{
    private IPipelineStepBuilder<IConsumeContext, IOutboxMessage>? _step;

    /// <inheritdoc />
    public IPipelineStep<IConsumeContext, IOutboxMessage> Build()
    {
        return new Pipeline<IConsumeContext, IOutboxMessage>(_step?.Build());
    }

    /// <inheritdoc />
    public IConsumePipelineStepBuilder<IOutboxMessage, TOut> AddStep<TOut>(
        Func<IOutboxMessage, IConsumeContext, ValueTask<TOut>> action)
    {
        if (_step != null) throw new InvalidOperationException("The first step is already added.");

        var step = new ConsumePipelineStepBuilder<IOutboxMessage, TOut>(action);
        _step = step;
        return step;
    }

    /// <inheritdoc />
    public IConsumePipelineStepBuilder<IOutboxMessage, TOut> AddSyncStep<TOut>(
        Func<IOutboxMessage, IConsumeContext, TOut> action)
    {
        if (_step != null) throw new InvalidOperationException("The first step is already added.");

        var step = new ConsumePipelineStepBuilder<IOutboxMessage, TOut>(action);
        _step = step;
        return step;
    }
}