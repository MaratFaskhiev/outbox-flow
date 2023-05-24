using System.Data;
using OutboxFlow.Abstractions;
using OutboxFlow.Pipelines;

namespace OutboxFlow.Configuration;

/// <summary>
/// Builds an outbox consume pipeline.
/// </summary>
public sealed class ConsumerBuilder : IPipelineStepBuilder<IConsumeContext, IOutboxMessage>
{
    private IPipelineStepBuilder<IConsumeContext, IOutboxMessage>? _step;

    /// <summary>
    /// Gets or sets the amount of messages to consume.
    /// </summary>
    public int BatchSize { get; set; } = 128;

    /// <summary>
    /// Gets or sets the delay between each attempt to consume messages.
    /// </summary>
    public TimeSpan ConsumeDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the transaction isolation level.
    /// </summary>
    public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.RepeatableRead;

    /// <summary>
    /// Builds a consume pipeline.
    /// </summary>
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