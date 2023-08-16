using Confluent.Kafka;
using Xunit;

namespace OutboxFlow.Kafka.IntegrationTests;

public sealed class KafkaProducerBuilderTests : IClassFixture<KafkaFixture>
{
    private const string Topic = "test_topic";
    private readonly string _bootstrapAddress;

    private readonly KafkaProducerBuilder _builder = new();

    public KafkaProducerBuilderTests(KafkaFixture kafkaFixture)
    {
        _bootstrapAddress = kafkaFixture.BootstrapAddress;
    }

    [Fact]
    public async Task ProduceMessageTest()
    {
        var key = Guid.NewGuid();
        var value = Guid.NewGuid();

        using var producer = _builder.Create(new ProducerConfig
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