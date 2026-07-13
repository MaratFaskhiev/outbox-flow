using Confluent.Kafka;

namespace OutboxFlow.Kafka;

/// <summary>
/// Extension methods for <see cref="ProducerConfig" />.
/// </summary>
public static class ProducerConfigExtensions
{
    /// <summary>
    /// Sets recommended outbox defaults: <see cref="ProducerConfig.EnableIdempotence" /> to <c>true</c>
    /// and <see cref="ProducerConfig.Acks" /> to <see cref="Acks.All" />.
    /// Existing values are not overwritten.
    /// </summary>
    /// <param name="config">Producer configuration.</param>
    /// <returns>The same <paramref name="config" /> instance for chaining.</returns>
    public static ProducerConfig WithOutboxDefaults(this ProducerConfig config)
    {
        config.EnableIdempotence ??= true;
        config.Acks ??= Acks.All;
        return config;
    }
}