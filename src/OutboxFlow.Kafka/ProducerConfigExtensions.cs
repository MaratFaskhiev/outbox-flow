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
    public static ProducerConfig ApplyOutboxDefaults(this ProducerConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        config.EnableIdempotence ??= true;
        config.Acks ??= Acks.All;
        return config;
    }

    /// <summary>
    /// Sets recommended batch defaults: <see cref="ProducerConfig.LingerMs" /> to <c>50</c>
    /// and <see cref="ProducerConfig.BatchSize" /> to <c>65536</c>.
    /// Existing values are not overwritten.
    /// </summary>
    /// <param name="config">Producer configuration.</param>
    /// <returns>The same <paramref name="config" /> instance for chaining.</returns>
    public static ProducerConfig ApplyBatchDefaults(this ProducerConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        config.LingerMs ??= 50;
        config.BatchSize ??= 65536;
        return config;
    }
}