using System.Collections.Concurrent;
using Confluent.Kafka;

namespace OutboxFlow.Kafka;

/// <inheritdoc cref="OutboxFlow.Kafka.IKafkaProducerRegistry" />
public sealed class KafkaProducerRegistry : IKafkaProducerRegistry, IDisposable, IAsyncDisposable
{
    private readonly ConcurrentDictionary<ProducerConfig, Lazy<IProducer<byte[], byte[]>>> _producers = new();
    private bool _disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var producer in _producers.Values.Where(x => x.IsValueCreated))
        {
            using (var flushCts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                try
                {
                    producer.Value.Flush(flushCts.Token);
                }
                catch (OperationCanceledException)
                {
                }
            }

            producer.Value.Dispose();
        }

        _producers.Clear();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var producer in _producers.Values.Where(x => x.IsValueCreated))
        {
            using var flushCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                await Task.Run(() => producer.Value.Flush(flushCts.Token), flushCts.Token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }

            producer.Value.Dispose();
        }

        _producers.Clear();

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public IProducer<byte[], byte[]> GetOrCreate(IKafkaProducerBuilder producerBuilder, ProducerConfig producerConfig)
    {
        return _producers.GetOrAdd(producerConfig, cfg => new Lazy<IProducer<byte[], byte[]>>(() =>
            producerBuilder.Create(cfg)
        )).Value;
    }

    /// <inheritdoc />
    public void Remove(ProducerConfig producerConfig)
    {
        _producers.TryRemove(producerConfig, out _);
    }
}