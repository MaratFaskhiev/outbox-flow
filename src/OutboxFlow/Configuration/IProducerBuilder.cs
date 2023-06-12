using Microsoft.Extensions.DependencyInjection;

namespace OutboxFlow.Configuration;

/// <summary>
/// Builds an outbox producer.
/// </summary>
public interface IProducerBuilder
{
    /// <summary>
    /// Gets or sets the registrar to register an outbox storage.
    /// </summary>
    IOutboxStorageRegistrar? OutboxStorageRegistrar { get; set; }

    /// <summary>
    /// Configures produce pipeline for the specified message type.
    /// </summary>
    /// <param name="configure">Configure action.</param>
    /// <typeparam name="T">Message type.</typeparam>
    IProducerBuilder ForMessage<T>(Action<IProducePipelineBuilder<T>> configure);

    /// <summary>
    /// Builds an outbox producer.
    /// </summary>
    /// <param name="services">Collection of service descriptors.</param>
    void Build(IServiceCollection services);
}