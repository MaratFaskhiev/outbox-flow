#pragma warning disable CA1819

namespace OutboxFlow.Abstractions;

/// <summary>
/// Outbox message.
/// </summary>
public interface IOutboxMessage
{
    /// <summary>
    /// Gets the destination.
    /// </summary>
    public string? Destination { get; }

    /// <summary>
    /// Gets the message key.
    /// </summary>
    public byte[]? Key { get; }

    /// <summary>
    /// Gets the message value.
    /// </summary>
    public byte[] Value { get; }
}

#pragma warning restore CA1819