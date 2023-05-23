using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OutboxFlow.Abstractions;
using OutboxFlow.Consume;
using OutboxFlow.Produce;

namespace OutboxFlow.Configuration;

/// <summary>
/// Builds outbox pipelines.
/// </summary>
public sealed class OutboxBuilder
{
    private Action<IServiceProvider, ConsumerBuilder>? _consumerConfigureAction;
    private Action<IServiceProvider, ProducerBuilder>? _producerConfigureAction;

    /// <summary>
    /// Configures outbox produce pipelines.
    /// </summary>
    /// <param name="configure">Configure action.</param>
    public OutboxBuilder AddProducer(Action<IServiceProvider, ProducerBuilder> configure)
    {
        if (_producerConfigureAction != null)
            throw new InvalidOperationException("An outbox producer is already configured.");
        _producerConfigureAction = configure;
        return this;
    }

    /// <summary>
    /// Configures outbox consume pipelines.
    /// </summary>
    /// <param name="configure">Configure action.</param>
    public OutboxBuilder AddConsumer(Action<IServiceProvider, ConsumerBuilder> configure)
    {
        if (_consumerConfigureAction != null)
            throw new InvalidOperationException("An outbox consumer is already configured.");
        _consumerConfigureAction = configure;
        return this;
    }

    /// <summary>
    /// Builds outbox pipelines.
    /// </summary>
    /// <param name="services">Collection of service descriptors.</param>
    public void Build(IServiceCollection services)
    {
        BuildProducer(services);
        BuildConsumer(services);
    }

    private static void BuildConsumer(IServiceCollection services)
    {
        services.AddHostedService<OutboxStorageConsumer>();
    }

    private void BuildProducer(IServiceCollection services)
    {
        if (_producerConfigureAction == null) return;

        services.TryAddSingleton(sp =>
        {
            var builder = new ProducerBuilder();
            _producerConfigureAction(sp, builder);
            return builder.BuildRegistry();
        });

        services.TryAddScoped<IProducer, Producer>();
    }
}