using System.Data;
using OutboxFlow.Consume;

namespace OutboxFlow.Configuration;

/// <summary>
/// Configuration options for the <see cref="OutboxConsumer" />.
/// </summary>
public sealed class OutboxStorageConsumerOptions
{
    /// <summary>
    /// Gets or sets the amount of messages to consume.
    /// </summary>
    public int BatchSize { get; set; }

    /// <summary>
    /// Gets or sets the delay between each attempt to consume messages.
    /// </summary>
    public TimeSpan ConsumeDelay { get; set; }

    /// <summary>
    /// Gets or sets the transaction isolation level.
    /// </summary>
    public IsolationLevel IsolationLevel { get; set; }
}