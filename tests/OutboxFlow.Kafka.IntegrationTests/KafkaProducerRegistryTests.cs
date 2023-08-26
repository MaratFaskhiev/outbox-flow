using Confluent.Kafka;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace OutboxFlow.Kafka.IntegrationTests;

public sealed class KafkaProducerRegistryTests : IClassFixture<KafkaFixture>, IDisposable
{
    private const string Topic = "test_topic";
    private readonly string _bootstrapAddress;

    private readonly KafkaProducerRegistry _registry = new();

    public KafkaProducerRegistryTests(KafkaFixture kafkaFixture)
    {
        _bootstrapAddress = kafkaFixture.BootstrapAddress;
    }

    public void Dispose()
    {
        _registry.Dispose();
    }

    [Fact]
    public async Task ProduceMessageTest()
    {
        var key = Guid.NewGuid();
        var value = Guid.NewGuid();

        var producer = _registry.GetOrCreate(
            new DefaultKafkaProducerBuilder(_registry, NullLogger<DefaultKafkaProducerBuilder>.Instance),
            new ProducerConfig
            {
                BootstrapServers = _bootstrapAddress
            });
        var kafkaMessage = new Message<byte[], byte[]>
        {
            Key = key.ToByteArray(),
            Value = value.ToByteArray()
        };
        var result = await producer.ProduceAsync(Topic, kafkaMessage);

        Assert.Equal(PersistenceStatus.Persisted, result.Status);

        using var consumer = new ConsumerBuilder<byte[], byte[]>(new ConsumerConfig
        {
            BootstrapServers = _bootstrapAddress,
            GroupId = "test_consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnablePartitionEof = true
        }).Build();
        consumer.Subscribe(Topic);
        var consumeResult = consumer.Consume(TimeSpan.FromSeconds(3));

        Assert.NotNull(consumeResult);
        Assert.Equal(key, new Guid(consumeResult.Message.Key));
        Assert.Equal(value, new Guid(consumeResult.Message.Value));
    }
}