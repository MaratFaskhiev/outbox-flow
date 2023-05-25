using OutboxFlow.Abstractions;
using OutboxFlow.Pipelines;

namespace OutboxFlow.Configuration;

/// <summary>
/// Outbox consume pipeline builder.
/// </summary>
public sealed class ConsumePipelineBuilder : IPipelineStepBuilder<IConsumeContext, IOutboxMessage>
{
    private IPipelineStepBuilder<IConsumeContext, IOutboxMessage>? _step;

    /// <inheritdoc />
    public IPipelineStep<IConsumeContext, IOutboxMessage> Build()
    {
        return new Pipeline<IConsumeContext, IOutboxMessage>(_step?.Build());
    }

    /// <summary>
    /// Adds the first step to the pipeline.
    /// </summary>
    /// <param name="action">Step.</param>
    /// <typeparam name="TOut">Output parameter type.</typeparam>
    public ConsumePipelineStepBuilder<IOutboxMessage, TOut> AddStep<TOut>(
        Func<IOutboxMessage, IConsumeContext, ValueTask<TOut>> action)
    {
        if (_step != null) throw new InvalidOperationException("The first step is already added.");

        var step = new ConsumePipelineStepBuilder<IOutboxMessage, TOut>(action);
        _step = step;
        return step;
    }

    /// <summary>
    /// Adds the first step to the pipeline.
    /// </summary>
    /// <param name="action">Step.</param>
    /// <typeparam name="TOut">Output parameter type.</typeparam>
    public ConsumePipelineStepBuilder<IOutboxMessage, TOut> AddSyncStep<TOut>(
        Func<IOutboxMessage, IConsumeContext, TOut> action)
    {
        if (_step != null) throw new InvalidOperationException("The first step is already added.");

        var step = new ConsumePipelineStepBuilder<IOutboxMessage, TOut>(action);
        _step = step;
        return step;
    }
}