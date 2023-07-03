using Microsoft.Extensions.DependencyInjection;
using OutboxFlow.Consume.Configuration;
using OutboxFlow.Produce.Configuration;

namespace OutboxFlow.Configuration;

/// <summary>
/// Builds outbox pipelines.
/// </summary>
public interface IOutboxBuilder
{
    /// <summary>
    /// Configures outbox produce pipelines.
    /// </summary>
    /// <param name="configure">Configure action.</param>
    IOutboxBuilder AddProducer(Action<IProducerBuilder> configure);

    /// <summary>
    /// Configures outbox consume pipelines.
    /// </summary>
    /// <param name="configure">Configure action.</param>
    IOutboxBuilder AddConsumer(Action<IConsumerBuilder> configure);

    /// <summary>
    /// Builds outbox pipelines.
    /// </summary>
    /// <param name="services">Collection of service descriptors.</param>
    void Build(IServiceCollection services);
}