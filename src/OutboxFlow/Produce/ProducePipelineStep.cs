using OutboxFlow.Abstractions;

namespace OutboxFlow.Produce;

/// <summary>
/// Outbox produce pipeline step.
/// </summary>
/// <typeparam name="TIn">Input parameter type.</typeparam>
/// <typeparam name="TOut">Output parameter type.</typeparam>
public sealed class ProducePipelineStep<TIn, TOut> : IProducePipelineStep<TIn>
{
    private readonly Func<TIn, IProduceContext, ValueTask<TOut>> _action;
    private readonly IProducePipelineStep<TOut>? _nextStep;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="action">Pipeline action.</param>
    /// <param name="nextStep">Next step.</param>
    public ProducePipelineStep(
        Func<TIn, IProduceContext, ValueTask<TOut>> action,
        IProducePipelineStep<TOut>? nextStep)
    {
        _action = action;
        _nextStep = nextStep;
    }

    /// <inheritdoc />
    public async ValueTask InvokeAsync(TIn message, IProduceContext context)
    {
        var result = await _action.Invoke(message, context).ConfigureAwait(false);

        if (_nextStep == null) return;

        await _nextStep.InvokeAsync(result, context).ConfigureAwait(false);
    }
}