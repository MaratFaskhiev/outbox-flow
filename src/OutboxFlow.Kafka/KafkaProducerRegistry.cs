using System.Collections.Concurrent;
using Confluent.Kafka;

namespace OutboxFlow.Kafka;

/// <inheritdoc cref="OutboxFlow.Kafka.IKafkaProducerRegistry" />
public class KafkaProducerRegistry : IKafkaProducerRegistry, IDisposable
{
    private readonly ConcurrentDictionary<ProducerConfig, Lazy<IProducer<byte[], byte[]>>> _producers = new();

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public IProducer<byte[], byte[]> GetOrCreate(ProducerConfig producerConfig)
    {
        return _producers.GetOrAdd(producerConfig, cfg => new Lazy<IProducer<byte[], byte[]>>(() =>
            new ProducerBuilder<byte[], byte[]>(cfg).Build()
        )).Value;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var producer in _producers.Values.Where(x => x.IsValueCreated)) producer.Value.Dispose();

            _producers.Clear();
        }
    }
}