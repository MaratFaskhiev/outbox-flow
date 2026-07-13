using System.Collections.Concurrent;
using System.Data;
using OutboxFlow.Produce;
using OutboxFlow.Storage;

namespace OutboxFlow.Sample;

internal sealed class InMemoryStorage : IOutboxStorage
{
    private readonly ConcurrentQueue<IOutboxMessage> _messages = new();

    public ValueTask<IReadOnlyCollection<IOutboxMessage>> FetchAsync(
        int batchSize,
        IDbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        var result = new List<IOutboxMessage>(batchSize);
        while (result.Count < batchSize && _messages.TryDequeue(out var message)) result.Add(message);

        return new ValueTask<IReadOnlyCollection<IOutboxMessage>>(result.AsReadOnly());
    }

    public ValueTask SaveAsync(IProduceContext context, CancellationToken cancellationToken = default)
    {
        var message = new InMemoryMessage(
            context.Destination!,
            new Dictionary<string, string>(context.Headers),
            context.Key,
            context.Value!);

        _messages.Enqueue(message);

        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteAsync(
        IReadOnlyCollection<IOutboxMessage> outboxMessages,
        IDbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }

    private sealed class InMemoryMessage : IOutboxMessage
    {
        public InMemoryMessage(
            string destination,
            IDictionary<string, string> headers,
            byte[]? key,
            byte[] value)
        {
            Destination = destination;
            Headers = headers;
            Key = key;
            Value = value;
        }

        public string? Destination { get; }

        public IDictionary<string, string> Headers { get; }

        public byte[]? Key { get; }

        public byte[] Value { get; }
    }
}