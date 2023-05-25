using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OutboxFlow.Abstractions;
using OutboxFlow.Produce;

namespace OutboxFlow.Configuration;

/// <summary>
/// Builds an outbox producer.
/// </summary>
public sealed class ProducerBuilder
{
    private readonly Dictionary<Type, object> _messagePipelines = new();

    /// <summary>
    /// Gets or sets the registrar to register an outbox storage.
    /// </summary>
    public IOutboxStorageRegistrar? OutboxStorageRegistrar { get; set; }

    /// <summary>
    /// Configures produce pipeline for the specified message type.
    /// </summary>
    /// <param name="configure">Configure action.</param>
    /// <typeparam name="T">Message type.</typeparam>
    public ProducerBuilder ForMessage<T>(Action<ProducePipelineBuilder<T>> configure)
    {
        if (_messagePipelines.ContainsKey(typeof(T)))
            throw new InvalidOperationException(
                $"Produce pipeline for the message type \"{typeof(T).Name}\" is already registered.");

        var pipelineBuilder = new ProducePipelineBuilder<T>();
        configure(pipelineBuilder);
        _messagePipelines.Add(typeof(T), pipelineBuilder.Build());

        return this;
    }

    /// <summary>
    /// Builds an outbox producer.
    /// </summary>
    /// <param name="services">Collection of service descriptors.</param>
    public void Build(IServiceCollection services)
    {
        if (OutboxStorageRegistrar == null)
            throw new InvalidOperationException("The outbox storage registrar must be configured.");

        var registry = new ProducePipelineRegistry(_messagePipelines);
        services.TryAddSingleton<IProducePipelineRegistry>(registry);
        services.TryAddScoped<IProducer, Producer>();

        OutboxStorageRegistrar.Register(services);
    }
}