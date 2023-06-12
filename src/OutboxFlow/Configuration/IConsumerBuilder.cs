using System.Data;
using Microsoft.Extensions.DependencyInjection;

namespace OutboxFlow.Configuration;

/// <summary>
/// Builds an outbox consumer.
/// </summary>
public interface IConsumerBuilder
{
    /// <summary>
    /// Gets or sets the registrar to register an outbox storage.
    /// </summary>
    IOutboxStorageRegistrar? OutboxStorageRegistrar { get; set; }

    /// <summary>
    /// Gets or sets the amount of messages to consume.
    /// </summary>
    int BatchSize { get; set; }

    /// <summary>
    /// Gets or sets the delay between each attempt to consume messages.
    /// </summary>
    TimeSpan ConsumeDelay { get; set; }

    /// <summary>
    /// Gets or sets the transaction isolation level.
    /// </summary>
    IsolationLevel IsolationLevel { get; set; }

    /// <summary>
    /// Get or sets the consume operation timeout.
    /// </summary>
    TimeSpan Timeout { get; set; }

    /// <summary>
    /// Configures the default consume pipeline.
    /// </summary>
    /// <param name="configure">Configure action.</param>
    IConsumerBuilder ByDefault(Action<IConsumePipelineBuilder> configure);

    /// <summary>
    /// Configures consume pipeline for the specified destination.
    /// </summary>
    /// <param name="destination">Destination.</param>
    /// <param name="configure">Configure action.</param>
    IConsumerBuilder ForDestination(string destination, Action<IConsumePipelineBuilder> configure);

    /// <summary>
    /// Builds an outbox consumer.
    /// </summary>
    /// <param name="services">Collection of service descriptors.</param>
    void Build(IServiceCollection services);
}