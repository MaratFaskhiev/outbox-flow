using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OutboxFlow.Abstractions;
using OutboxFlow.Consume;
using OutboxFlow.Pipelines;

namespace OutboxFlow.Configuration;

/// <summary>
/// Builds an outbox consumer.
/// </summary>
public sealed class ConsumerBuilder
{
    private IPipelineStepBuilder<IConsumeContext, IOutboxMessage>? _step;

    /// <summary>
    /// Gets or sets the registrar to register an outbox storage.
    /// </summary>
    public IOutboxStorageRegistrar? OutboxStorageRegistrar { get; set; }

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
    /// Builds an outbox consumer.
    /// </summary>
    /// <param name="services">Collection of service descriptors.</param>
    public void Build(IServiceCollection services)
    {
        if (OutboxStorageRegistrar == null)
            throw new InvalidOperationException("The outbox storage registrar must be configured.");

        services.AddOptions<OutboxStorageConsumerOptions>().Configure(options =>
        {
            options.IsolationLevel = IsolationLevel;
            options.BatchSize = BatchSize;
            options.ConsumeDelay = ConsumeDelay;
        });

        var pipeline = new Pipeline<IConsumeContext, IOutboxMessage>(_step?.Build());
        services.TryAddSingleton<IPipelineStep<IConsumeContext, IOutboxMessage>>(pipeline);

        services.AddHostedService<OutboxConsumer>();

        OutboxStorageRegistrar.Register(services);
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