﻿using System.Collections.Concurrent;
using Confluent.Kafka;

namespace OutboxFlow.Kafka;

/// <inheritdoc cref="OutboxFlow.Kafka.IKafkaProducerRegistry" />
public sealed class KafkaProducerRegistry : IKafkaProducerRegistry, IDisposable
{
    private readonly IKafkaProducerBuilder _builder;
    private readonly ConcurrentDictionary<ProducerConfig, Lazy<IProducer<byte[], byte[]>>> _producers = new();

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="builder">Kafka producer builder.</param>
    public KafkaProducerRegistry(IKafkaProducerBuilder builder)
    {
        _builder = builder;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var producer in _producers.Values.Where(x => x.IsValueCreated)) producer.Value.Dispose();

        _producers.Clear();
    }

    /// <inheritdoc />
    public IProducer<byte[], byte[]> GetOrCreate(ProducerConfig producerConfig)
    {
        return _producers.GetOrAdd(producerConfig, cfg => new Lazy<IProducer<byte[], byte[]>>(() =>
            _builder.Create(cfg)
        )).Value;
    }
}