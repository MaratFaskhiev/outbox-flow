using OutboxFlow.Abstractions;
using OutboxFlow.Produce;

namespace OutboxFlow.Configuration;

/// <summary>
/// Outbox produce pipeline step builder.
/// </summary>
/// <typeparam name="TIn">Input parameter type.</typeparam>
/// <typeparam name="TOut">Output parameter type.</typeparam>
public sealed class ProducePipelineStepBuilder<TIn, TOut> : IProducePipelineStepBuilder<TIn>
{
    private readonly Func<TIn, IProduceContext, ValueTask<TOut>> _action;
    private IProducePipelineStepBuilder<TOut>? _nextStep;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="action">Pipeline action.</param>
    public ProducePipelineStepBuilder(Func<TIn, IProduceContext, ValueTask<TOut>> action)
    {
        _action = action;
    }

    /// <inheritdoc />
    public IProducePipelineStep<TIn> Build()
    {
        return new ProducePipelineStep<TIn, TOut>(_action, _nextStep?.Build());
    }

    /// <summary>
    /// Adds step to the pipeline.
    /// </summary>
    /// <param name="action">Step.</param>
    /// <typeparam name="TNext">Output parameter type.</typeparam>
    public ProducePipelineStepBuilder<TOut, TNext> AddStep<TNext>(
        Func<TOut, IProduceContext, ValueTask<TNext>> action)
    {
        var nextStep = new ProducePipelineStepBuilder<TOut, TNext>(action);
        _nextStep = nextStep;
        return nextStep;
    }
}