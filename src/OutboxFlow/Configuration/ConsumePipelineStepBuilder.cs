using OutboxFlow.Abstractions;
using OutboxFlow.Pipelines;

namespace OutboxFlow.Configuration;

/// <summary>
/// Outbox consume pipeline step builder.
/// </summary>
/// <typeparam name="TIn">Input parameter type.</typeparam>
/// <typeparam name="TOut">Output parameter type.</typeparam>
public sealed class ConsumePipelineStepBuilder<TIn, TOut> : IPipelineStepBuilder<IConsumeContext, TIn>
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

    /// <summary>
    /// Adds a step to the pipeline.
    /// </summary>
    /// <param name="action">Step.</param>
    /// <typeparam name="TNext">Output parameter type.</typeparam>
    public ConsumePipelineStepBuilder<TOut, TNext> AddStep<TNext>(
        Func<TOut, IConsumeContext, ValueTask<TNext>> action)
    {
        var nextStep = new ConsumePipelineStepBuilder<TOut, TNext>(action);
        _nextStep = nextStep;
        return nextStep;
    }

    /// <summary>
    /// Adds a synchronous step to the pipeline.
    /// </summary>
    /// <param name="action">Step.</param>
    /// <typeparam name="TNext">Output parameter type.</typeparam>
    public ConsumePipelineStepBuilder<TOut, TNext> AddStep<TNext>(
        Func<TOut, IConsumeContext, TNext> action)
    {
        var nextStep = new ConsumePipelineStepBuilder<TOut, TNext>(action);
        _nextStep = nextStep;
        return nextStep;
    }
}