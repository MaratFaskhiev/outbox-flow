using OutboxFlow.Configuration;

namespace OutboxFlow.Consume.Configuration;

/// <summary>
/// Outbox consume pipeline step builder.
/// </summary>
/// <typeparam name="TIn">Input parameter type.</typeparam>
/// <typeparam name="TOut">Output parameter type.</typeparam>
public sealed class ConsumePipelineStepBuilder<TIn, TOut> : IConsumePipelineStepBuilder<TIn, TOut>
{
    private readonly Func<TIn, IConsumeContext, ValueTask<TOut>>? _action;
    private readonly Func<TIn, IConsumeContext, TOut>? _syncAction;
    private IPipelineStepBuilder<IConsumeContext, TOut>? _nextStep;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="action">Pipeline action.</param>
    public ConsumePipelineStepBuilder(Func<TIn, IConsumeContext, ValueTask<TOut>> action)
    {
        _action = action;
    }

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="action">Pipeline action.</param>
    public ConsumePipelineStepBuilder(Func<TIn, IConsumeContext, TOut> action)
    {
        _syncAction = action;
    }

    /// <inheritdoc />
    public IPipelineStep<IConsumeContext, TIn> Build()
    {
        return _action != null
            ? new PipelineStep<IConsumeContext, TIn, TOut>(_action, _nextStep?.Build())
            : new PipelineStep<IConsumeContext, TIn, TOut>(_syncAction!, _nextStep?.Build());
    }

    /// <inheritdoc />
    public IConsumePipelineStepBuilder<TOut, TNext> AddStep<TNext>(
        Func<TOut, IConsumeContext, ValueTask<TNext>> action)
    {
        var nextStep = new ConsumePipelineStepBuilder<TOut, TNext>(action);
        _nextStep = nextStep;
        return nextStep;
    }

    /// <inheritdoc />
    public IConsumePipelineStepBuilder<TOut, TNext> AddSyncStep<TNext>(
        Func<TOut, IConsumeContext, TNext> action)
    {
        var nextStep = new ConsumePipelineStepBuilder<TOut, TNext>(action);
        _nextStep = nextStep;
        return nextStep;
    }
}