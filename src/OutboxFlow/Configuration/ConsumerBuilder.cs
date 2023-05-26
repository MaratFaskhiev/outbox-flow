using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OutboxFlow.Abstractions;
using OutboxFlow.Consume;

namespace OutboxFlow.Configuration;

/// <summary>
/// Builds an outbox consumer.
/// </summary>
public sealed class ConsumerBuilder
{
    private readonly Dictionary<string, IPipelineStep<IConsumeContext, IOutboxMessage>> _destinationPipelines
        = new(StringComparer.OrdinalIgnoreCase);

    private IPipelineStep<IConsumeContext, IOutboxMessage>? _defaultPipeline;

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
    public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;

    /// <summary>
    /// Get or sets the consume operation timeout.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Configures the default consume pipeline.
    /// </summary>
    /// <param name="configure">Configure action.</param>
    public ConsumerBuilder Default(Action<ConsumePipelineBuilder> configure)
    {
        if (_defaultPipeline != null)
            throw new InvalidOperationException(
                "Default consume pipeline is already registered.");

        var pipelineBuilder = new ConsumePipelineBuilder();
        configure(pipelineBuilder);
        _defaultPipeline = pipelineBuilder.Build();

        return this;
    }

    /// <summary>
    /// Configures consume pipeline for the specified destination.
    /// </summary>
    /// <param name="destination">Destination.</param>
    /// <param name="configure">Configure action.</param>
    public ConsumerBuilder ForDestination(string destination, Action<ConsumePipelineBuilder> configure)
    {
        if (_destinationPipelines.ContainsKey(destination))
            throw new InvalidOperationException(
                $"Consume pipeline for the destination \"{destination}\" is already registered.");

        var pipelineBuilder = new ConsumePipelineBuilder();
        configure(pipelineBuilder);
        _destinationPipelines.Add(destination, pipelineBuilder.Build());

        return this;
    }

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
            options.Timeout = Timeout;
        });

        var registry = new ConsumePipelineRegistry(_destinationPipelines, _defaultPipeline);
        services.TryAddSingleton<IConsumePipelineRegistry>(registry);
        services.TryAddScoped<IOutboxConsumer, OutboxConsumer>();
        services.AddHostedService<OutboxConsumerService>();

        OutboxStorageRegistrar.Register(services);
    }
}