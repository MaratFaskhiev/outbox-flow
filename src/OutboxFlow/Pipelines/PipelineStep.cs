using OutboxFlow.Abstractions;

namespace OutboxFlow.Pipelines;

/// <summary>
/// Outbox pipeline step.
/// </summary>
/// <typeparam name="TContext">Context type.</typeparam>
/// <typeparam name="TIn">Input parameter type.</typeparam>
/// <typeparam name="TOut">Output parameter type.</typeparam>
public sealed class PipelineStep<TContext, TIn, TOut> : IPipelineStep<TContext, TIn>
{
    private readonly Func<TIn, TContext, ValueTask<TOut>>? _action;
    private readonly IPipelineStep<TContext, TOut>? _nextStep;
    private readonly Func<TIn, TContext, TOut>? _syncAction;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="action">Pipeline action.</param>
    /// <param name="nextStep">Next step.</param>
    public PipelineStep(
        Func<TIn, TContext, ValueTask<TOut>> action,
        IPipelineStep<TContext, TOut>? nextStep)
    {
        _action = action;
        _nextStep = nextStep;
    }

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="action">Pipeline action.</param>
    /// <param name="nextStep">Next step.</param>
    public PipelineStep(
        Func<TIn, TContext, TOut> action,
        IPipelineStep<TContext, TOut>? nextStep)
    {
        _syncAction = action;
        _nextStep = nextStep;
    }

    /// <inheritdoc />
    public async ValueTask InvokeAsync(TIn message, TContext context)
    {
        var result = _action != null
            ? await _action.Invoke(message, context).ConfigureAwait(false)
            : _syncAction!.Invoke(message, context);

        if (_nextStep == null) return;

        await _nextStep.InvokeAsync(result, context).ConfigureAwait(false);
    }
}