using System.Data;
using OutboxFlow.Storage.Configuration;

namespace OutboxFlow.Consume.Configuration;

/// <summary>
/// Extension methods for configuring the consumer.
/// </summary>
public static class ConsumerBuilderExtensions
{
    /// <summary>
    /// Sets the registrar to register an outbox storage.
    /// </summary>
    /// <param name="consumerBuilder">Consumer builder.</param>
    /// <param name="registrar">Registrar.</param>
    public static IConsumerBuilder SetOutboxStorageRegistrar(
        this IConsumerBuilder consumerBuilder,
        IOutboxStorageRegistrar registrar)
    {
        ArgumentNullException.ThrowIfNull(consumerBuilder);
        consumerBuilder.OutboxStorageRegistrar = registrar;

        return consumerBuilder;
    }

    /// <summary>
    /// Sets the amount of messages to consume.
    /// </summary>
    /// <param name="consumerBuilder">Consumer builder.</param>
    /// <param name="batchSize">Batch size.</param>
    public static IConsumerBuilder SetBatchSize(
        this IConsumerBuilder consumerBuilder,
        int batchSize)
    {
        ArgumentNullException.ThrowIfNull(consumerBuilder);
        consumerBuilder.BatchSize = batchSize;

        return consumerBuilder;
    }

    /// <summary>
    /// Sets the delay between each attempt to consume messages.
    /// </summary>
    /// <param name="consumerBuilder">Consumer builder.</param>
    /// <param name="consumeDelay">Consume delay.</param>
    public static IConsumerBuilder SetConsumeDelay(
        this IConsumerBuilder consumerBuilder,
        TimeSpan consumeDelay)
    {
        ArgumentNullException.ThrowIfNull(consumerBuilder);
        consumerBuilder.ConsumeDelay = consumeDelay;

        return consumerBuilder;
    }

    /// <summary>
    /// Sets the transaction isolation level.
    /// </summary>
    /// <param name="consumerBuilder">Consumer builder.</param>
    /// <param name="isolationLevel">Isolation level.</param>
    public static IConsumerBuilder SetIsolationLevel(
        this IConsumerBuilder consumerBuilder,
        IsolationLevel isolationLevel)
    {
        ArgumentNullException.ThrowIfNull(consumerBuilder);
        consumerBuilder.IsolationLevel = isolationLevel;

        return consumerBuilder;
    }

    /// <summary>
    /// Sets the consume operation timeout.
    /// </summary>
    /// <param name="consumerBuilder">Consumer builder.</param>
    /// <param name="timeout">Timeout.</param>
    public static IConsumerBuilder SetTimeout(
        this IConsumerBuilder consumerBuilder,
        TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(consumerBuilder);
        consumerBuilder.Timeout = timeout;

        return consumerBuilder;
    }
}