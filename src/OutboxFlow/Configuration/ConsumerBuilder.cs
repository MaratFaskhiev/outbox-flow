using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OutboxFlow.Abstractions;
using OutboxFlow.Consume;

namespace OutboxFlow.Configuration;

/// <inheritdoc />
public sealed class ConsumerBuilder : IConsumerBuilder
{
    private readonly Dictionary<string, IPipelineStep<IConsumeContext, IOutboxMessage>> _destinationPipelines
        = new(StringComparer.OrdinalIgnoreCase);

    private IPipelineStep<IConsumeContext, IOutboxMessage>? _defaultPipeline;

    /// <inheritdoc />
    public IOutboxStorageRegistrar? OutboxStorageRegistrar { get; set; }

    /// <inheritdoc />
    public int BatchSize { get; set; } = 128;

    /// <inheritdoc />
    public TimeSpan ConsumeDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <inheritdoc />
    public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;

    /// <inheritdoc />
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <inheritdoc />
    public IConsumerBuilder ByDefault(Action<IConsumePipelineBuilder> configure)
    {
        if (_defaultPipeline != null)
            throw new InvalidOperationException(
                "Default consume pipeline is already registered.");

        var pipelineBuilder = new ConsumePipelineBuilder();
        configure(pipelineBuilder);
        _defaultPipeline = pipelineBuilder.Build();

        return this;
    }

    /// <inheritdoc />
    public IConsumerBuilder ForDestination(string destination, Action<IConsumePipelineBuilder> configure)
    {
        if (_destinationPipelines.ContainsKey(destination))
            throw new InvalidOperationException(
                $"Consume pipeline for the destination \"{destination}\" is already registered.");

        var pipelineBuilder = new ConsumePipelineBuilder();
        configure(pipelineBuilder);
        _destinationPipelines.Add(destination, pipelineBuilder.Build());

        return this;
    }

    /// <inheritdoc />
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