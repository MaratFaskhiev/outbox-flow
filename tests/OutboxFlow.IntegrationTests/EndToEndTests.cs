using System.Text;
using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Npgsql;
using OutboxFlow.Produce;
using OutboxFlow.Kafka;
using OutboxFlow.Postgres;
using Xunit;

namespace OutboxFlow.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class EndToEndTests : IClassFixture<EndToEndFixture>, IDisposable
{
    private const string Topic = "e2e_test_topic";
    private readonly EndToEndFixture _fixture;
    private readonly OutboxStorage _storage = new();
    private readonly KafkaProducerRegistry _registry = new();

    public EndToEndTests(EndToEndFixture fixture)
    {
        _fixture = fixture;
    }

    public void Dispose()
    {
        _registry.Dispose();
    }

    [Fact]
    public async Task SaveFetchProduceConsumeEndToEndTest()
    {
        var key = Guid.NewGuid();
        var value = Guid.NewGuid();

        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var produceContext = new ProduceContext(
            transaction, Mock.Of<IServiceProvider>(), CancellationToken.None)
        {
            Destination = Topic,
            Key = key.ToByteArray(),
            Value = value.ToByteArray()
        };
        await _storage.SaveAsync(produceContext);

        var messages = await _storage.FetchAsync(100, transaction, CancellationToken.None);
        messages.Count.Should().Be(1);
        var message = messages.First();

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _fixture.BootstrapAddress
        };
        var producer = _registry.GetOrCreate(
            new DefaultKafkaProducerBuilder(
                _registry, NullLogger<DefaultKafkaProducerBuilder>.Instance),
            producerConfig);

        var kafkaMessage = new Message<byte[], byte[]>
        {
            Key = message.Key,
            Value = message.Value
        };
        if (message.Headers.Count != 0)
        {
            kafkaMessage.Headers = new Headers();
            foreach (var header in message.Headers)
            {
                kafkaMessage.Headers.Add(header.Key, Encoding.UTF8.GetBytes(header.Value));
            }
        }

        var result = await producer.ProduceAsync(Topic, kafkaMessage);
        result.Status.Should().Be(PersistenceStatus.Persisted);

        using var consumer = new ConsumerBuilder<byte[], byte[]>(new ConsumerConfig
        {
            BootstrapServers = _fixture.BootstrapAddress,
            GroupId = $"e2e_test_consumer_{Guid.NewGuid()}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnablePartitionEof = true
        }).Build();
        consumer.Subscribe(Topic);

        ConsumeResult<byte[], byte[]>? consumeResult = null;
        var retries = 10;
        while (consumeResult is null && retries-- > 0)
        {
            consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(500));
        }

        consumeResult.Should().NotBeNull();
        consumeResult!.Message.Key.Should().BeEquivalentTo(key.ToByteArray());
        consumeResult.Message.Value.Should().BeEquivalentTo(value.ToByteArray());

        await _storage.DeleteAsync(messages, transaction, CancellationToken.None);
        await transaction.CommitAsync();
    }
}