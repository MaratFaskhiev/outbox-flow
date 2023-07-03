using System.Data;
using OutboxFlow.Consume;

namespace OutboxFlow.Storage.Configuration;

/// <summary>
/// Configuration options for the <see cref="OutboxConsumerService" />.
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

    /// <summary>
    /// Get or sets the consume operation timeout.
    /// </summary>
    public TimeSpan Timeout { get; set; }
}