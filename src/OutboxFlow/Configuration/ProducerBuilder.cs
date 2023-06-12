using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OutboxFlow.Abstractions;
using OutboxFlow.Produce;

namespace OutboxFlow.Configuration;

/// <inheritdoc />
public sealed class ProducerBuilder : IProducerBuilder
{
    private readonly Dictionary<Type, object> _messagePipelines = new();

    /// <inheritdoc />
    public IOutboxStorageRegistrar? OutboxStorageRegistrar { get; set; }

    /// <inheritdoc />
    public IProducerBuilder ForMessage<T>(Action<IProducePipelineBuilder<T>> configure)
    {
        if (_messagePipelines.ContainsKey(typeof(T)))
            throw new InvalidOperationException(
                $"Produce pipeline for the message type \"{typeof(T).Name}\" is already registered.");

        var pipelineBuilder = new ProducePipelineBuilder<T>();
        configure(pipelineBuilder);
        _messagePipelines.Add(typeof(T), pipelineBuilder.Build());

        return this;
    }

    /// <inheritdoc />
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