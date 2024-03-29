﻿using OutboxFlow.Configuration;

namespace OutboxFlow.Produce.Configuration;

/// <summary>
/// Outbox produce pipeline step builder.
/// </summary>
/// <typeparam name="TIn">Input parameter type.</typeparam>
/// <typeparam name="TOut">Output parameter type.</typeparam>
public sealed class ProducePipelineStepBuilder<TIn, TOut> : IProducePipelineStepBuilder<TIn, TOut>
{
    private readonly Func<TIn, IProduceContext, ValueTask<TOut>>? _action;
    private readonly Func<TIn, IProduceContext, TOut>? _syncAction;
    private IPipelineStepBuilder<IProduceContext, TOut>? _nextStep;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="action">Pipeline action.</param>
    public ProducePipelineStepBuilder(Func<TIn, IProduceContext, ValueTask<TOut>> action)
    {
        _action = action;
    }

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="action">Pipeline action.</param>
    public ProducePipelineStepBuilder(Func<TIn, IProduceContext, TOut> action)
    {
        _syncAction = action;
    }

    /// <inheritdoc />
    public IPipelineStep<IProduceContext, TIn> Build()
    {
        return _action != null
            ? new PipelineStep<IProduceContext, TIn, TOut>(_action, _nextStep?.Build())
            : new PipelineStep<IProduceContext, TIn, TOut>(_syncAction!, _nextStep?.Build());
    }

    /// <summary>
    /// Adds a step to the pipeline.
    /// </summary>
    /// <param name="action">Step.</param>
    /// <typeparam name="TNext">Output parameter type.</typeparam>
    public IProducePipelineStepBuilder<TOut, TNext> AddAsyncStep<TNext>(
        Func<TOut, IProduceContext, ValueTask<TNext>> action)
    {
        if (_nextStep != null) throw new InvalidOperationException("The next step is already added.");

        var nextStep = new ProducePipelineStepBuilder<TOut, TNext>(action);
        _nextStep = nextStep;
        return nextStep;
    }

    /// <summary>
    /// Adds a synchronous step to the pipeline.
    /// </summary>
    /// <param name="action">Step.</param>
    /// <typeparam name="TNext">Output parameter type.</typeparam>
    public IProducePipelineStepBuilder<TOut, TNext> AddSyncStep<TNext>(
        Func<TOut, IProduceContext, TNext> action)
    {
        if (_nextStep != null) throw new InvalidOperationException("The next step is already added.");

        var nextStep = new ProducePipelineStepBuilder<TOut, TNext>(action);
        _nextStep = nextStep;
        return nextStep;
    }
}