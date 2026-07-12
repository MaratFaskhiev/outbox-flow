using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace OutboxFlow.Kafka.IntegrationTests;

[Trait("Category", "Integration")]
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
                BootstrapServers = _bootstrapAddress,
                MessageTimeoutMs = 5000,
                SocketTimeoutMs = 5000
            });
        var kafkaMessage = new Message<byte[], byte[]>
        {
            Key = key.ToByteArray(),
            Value = value.ToByteArray()
        };
        var result = await producer.ProduceAsync(Topic, kafkaMessage);

        result.Status.Should().Be(PersistenceStatus.Persisted);

        using var consumer = new ConsumerBuilder<byte[], byte[]>(new ConsumerConfig
        {
            BootstrapServers = _bootstrapAddress,
            GroupId = $"test_consumer_{Guid.NewGuid()}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnablePartitionEof = true
        }).Build();
        consumer.Subscribe(Topic);

        ConsumeResult<byte[], byte[]>? consumeResult = null;
        var retries = 10;
        while (consumeResult is null && retries-- > 0) consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(500));

        consumeResult.Should().NotBeNull();
        consumeResult!.Message.Key.Should().BeEquivalentTo(key.ToByteArray());
        consumeResult.Message.Value.Should().BeEquivalentTo(value.ToByteArray());
    }

    [Fact]
    public async Task CacheHitTest()
    {
        var config = new ProducerConfig
            {BootstrapServers = _bootstrapAddress, MessageTimeoutMs = 5000, SocketTimeoutMs = 5000};

        var producer1 = _registry.GetOrCreate(
            new DefaultKafkaProducerBuilder(_registry, NullLogger<DefaultKafkaProducerBuilder>.Instance),
            config);
        var producer2 = _registry.GetOrCreate(
            new DefaultKafkaProducerBuilder(_registry, NullLogger<DefaultKafkaProducerBuilder>.Instance),
            config);

        producer1.Should().BeSameAs(producer2);
    }

    [Fact]
    public async Task CacheMissTest()
    {
        var config1 = new ProducerConfig
        {
            BootstrapServers = _bootstrapAddress, ClientId = "client1", MessageTimeoutMs = 5000, SocketTimeoutMs = 5000
        };
        var config2 = new ProducerConfig
        {
            BootstrapServers = _bootstrapAddress, ClientId = "client2", MessageTimeoutMs = 5000, SocketTimeoutMs = 5000
        };

        var producer1 = _registry.GetOrCreate(
            new DefaultKafkaProducerBuilder(_registry, NullLogger<DefaultKafkaProducerBuilder>.Instance),
            config1);
        var producer2 = _registry.GetOrCreate(
            new DefaultKafkaProducerBuilder(_registry, NullLogger<DefaultKafkaProducerBuilder>.Instance),
            config2);

        producer1.Should().NotBeSameAs(producer2);
    }

    [Fact]
    public async Task RemoveWithoutDisposeTest()
    {
        var config = new ProducerConfig
            {BootstrapServers = _bootstrapAddress, MessageTimeoutMs = 5000, SocketTimeoutMs = 5000};

        var producer = _registry.GetOrCreate(
            new DefaultKafkaProducerBuilder(_registry, NullLogger<DefaultKafkaProducerBuilder>.Instance),
            config);

        _registry.Remove(config);

        var message = new Message<byte[], byte[]>
        {
            Key = Guid.NewGuid().ToByteArray(),
            Value = Guid.NewGuid().ToByteArray()
        };
        var result = await producer.ProduceAsync("test_topic_remove", message);
        result.Status.Should().Be(PersistenceStatus.Persisted);
    }
}