using OutboxFlow.Consume;

namespace OutboxFlow.Configuration;

/// <summary>
/// Configuration options for the <see cref="OutboxStorageConsumer" />.
/// </summary>
public sealed class OutboxStorageConsumerOptions
{
    /// <summary>
    /// Gets or sets the amount of messages to consume.
    /// </summary>
    public int BatchSize { get; set; } = 128;

    /// <summary>
    /// Gets or sets the delay between each attempt to consume messages.
    /// </summary>
    public TimeSpan ConsumeDelay { get; set; } = TimeSpan.FromSeconds(5);
}