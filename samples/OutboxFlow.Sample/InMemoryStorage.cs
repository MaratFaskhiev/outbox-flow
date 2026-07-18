using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using OutboxFlow.Consume.Configuration;
using OutboxFlow.Produce;
using OutboxFlow.Produce.Configuration;
using OutboxFlow.Storage;
using OutboxFlow.Storage.Configuration;

namespace OutboxFlow.Sample;

#region docs_storage_impl
internal sealed class InMemoryStorage : IOutboxStorage
{
    private readonly ConcurrentQueue<IOutboxMessage> _messages = new();

    public ValueTask<IReadOnlyCollection<IOutboxMessage>> FetchAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var result = new List<IOutboxMessage>(batchSize);
        while (result.Count < batchSize && _messages.TryDequeue(out var message)) result.Add(message);

        return new ValueTask<IReadOnlyCollection<IOutboxMessage>>(result.AsReadOnly());
    }

    public ValueTask SaveAsync(IProduceContext context)
    {
        var message = new InMemoryMessage(
            context.Destination!,
            new Dictionary<string, string>(context.Headers),
            context.Key,
            context.Value!);

        _messages.Enqueue(message);

        return ValueTask.CompletedTask;
    }

    public ValueTask SaveBatchAsync(
        IReadOnlyCollection<IProduceContext> contexts)
    {
        foreach (var context in contexts)
        {
            var message = new InMemoryMessage(
                context.Destination!,
                new Dictionary<string, string>(context.Headers),
                context.Key,
                context.Value!);

            _messages.Enqueue(message);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteAsync(
        IReadOnlyCollection<IOutboxMessage> outboxMessages,
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
#endregion

#region docs_storage_registrar
internal sealed class InMemoryStorageRegistrar : IOutboxStorageRegistrar
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IOutboxStorage, InMemoryStorage>();
    }
}

internal static class ProducerBuilderExtensions
{
    public static IProducerBuilder UseInMemory(this IProducerBuilder builder)
    {
        builder.OutboxStorageRegistrar = new InMemoryStorageRegistrar();
        return builder;
    }
}

internal static class ConsumerBuilderExtensions
{
    public static IConsumerBuilder UseInMemory(this IConsumerBuilder builder)
    {
        builder.SetOutboxStorageRegistrar(new InMemoryStorageRegistrar());
        return builder;
    }
}
#endregion