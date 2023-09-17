using OutboxFlow.Storage;

namespace OutboxFlow.Postgres;

/// <inheritdoc />
public sealed class OutboxMessage : IOutboxMessage
{
    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="id">ID.</param>
    /// <param name="destination">Destination.</param>
    /// <param name="headers">Headers.</param>
    /// <param name="key">Message key.</param>
    /// <param name="value">Message value.</param>
    public OutboxMessage(long id, string? destination, IDictionary<string, string> headers, byte[]? key, byte[] value)
    {
        Id = id;
        Destination = destination;
        Headers = headers;
        Key = key;
        Value = value;
    }

    /// <summary>
    /// Gets the message ID.
    /// </summary>
    public long Id { get; }

    /// <inheritdoc />
    public string? Destination { get; }

    /// <inheritdoc />
    public IDictionary<string, string> Headers { get; set; }

#pragma warning disable CA1819
    /// <inheritdoc />
    public byte[]? Key { get; }

    /// <inheritdoc />
    public byte[] Value { get; }
#pragma warning restore CA1819
}