using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OutboxFlow.Kafka;

/// <inheritdoc />
public sealed partial class DefaultKafkaProducerBuilder : IKafkaProducerBuilder
{
    private readonly ILogger<DefaultKafkaProducerBuilder> _logger;
    private readonly IKafkaProducerRegistry _registry;

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
            Log.FatalError(_logger, producer.Name, error.Code, error.Reason);

            _registry.Remove(producerConfig);
        }
        else
        {
            Log.NonFatalError(_logger, producer.Name, error.Code, error.Reason);
        }
    }
}