using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OutboxFlow.Kafka;

/// <inheritdoc />
public sealed class DefaultKafkaProducerBuilder : IKafkaProducerBuilder
{
    private readonly IKafkaProducerRegistry _registry;
    private readonly ILogger<DefaultKafkaProducerBuilder> _logger;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="registry">Producer registry.</param>
    /// <param name="logger">Logger.</param>
    public DefaultKafkaProducerBuilder(IKafkaProducerRegistry registry, ILogger<DefaultKafkaProducerBuilder> logger)
    {
        _registry = registry;
        _logger = logger;
    }

    /// <inheritdoc />
    public IProducer<byte[], byte[]> Create(ProducerConfig producerConfig)
    {
        return new ProducerBuilder<byte[], byte[]>(producerConfig)
            .SetErrorHandler((producer, error) => OnError(producer, producerConfig, error))
            .Build();
    }

    internal void OnError(IProducer<byte[], byte[]> producer, ProducerConfig producerConfig, Error error)
    {
        if (error.IsFatal)
        {
            _logger.LogError(
                "Kafka producer {name} error: {code} ({reason})",
                producer.Name,
                error.Code,
                error.Reason);

            _registry.Remove(producerConfig);
        }
        else
            _logger.LogWarning(
                "Kafka producer {name} error: {code} ({reason})",
                producer.Name,
                error.Code,
                error.Reason);
    }
}