using OutboxFlow.Abstractions;

namespace OutboxFlow;

/// <summary>
/// Outbox produce pipeline step.
/// </summary>
/// <typeparam name="TIn">Input parameter type.</typeparam>
/// <typeparam name="TOut">Output parameter type.</typeparam>
public sealed class ProducePipelineStep<TIn, TOut> : IProducePipelineStep<TIn>
{
    private readonly Func<TIn, IProduceContext, ValueTask<TOut>> _action;
    private IProducePipelineStep<TOut>? _nextStep;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="action">Pipeline action.</param>
    public ProducePipelineStep(Func<TIn, IProduceContext, ValueTask<TOut>> action)
    {
        _action = action;
    }

    /// <summary>
    /// Adds step to the pipeline
    /// </summary>
    /// <param name="action">Step.</param>
    public ProducePipelineStep<TOut, TNext> AddStep<TNext>(
        Func<TOut, IProduceContext, ValueTask<TNext>> action)
    {
        var nextStep = new ProducePipelineStep<TOut, TNext>(action);
        _nextStep = nextStep;
        return nextStep;
    }

    /// <inheritdoc />
    public async ValueTask InvokeAsync(TIn message, IProduceContext context)
    {
        var result = await _action.Invoke(message, context).ConfigureAwait(false);

        if (_nextStep == null)
        {
            return;
        }

        await _nextStep.InvokeAsync(result, context).ConfigureAwait(false);
    }
}