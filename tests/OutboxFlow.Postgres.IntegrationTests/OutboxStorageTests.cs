using FluentAssertions;
using Moq;
using Npgsql;
using OutboxFlow.Produce;
using OutboxFlow.Storage;
using Xunit;

namespace OutboxFlow.Postgres.IntegrationTests;

[Collection("PostgresIntegration")]
[Trait("Category", "Integration")]
public sealed class OutboxStorageTests : IAsyncLifetime
{
    private readonly string _connectionString;

    private readonly OutboxStorage _storage = new();

    private readonly List<NpgsqlTransaction> _transactions = [];

    public OutboxStorageTests(DatabaseFixture databaseFixture)
    {
        _connectionString = databaseFixture.ConnectionString;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (var transaction in _transactions) await transaction.DisposeAsync();
    }

    [Fact]
    public async Task MessageWorkflowTest()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        _transactions.Add(transaction);

        var produceContext = new ProduceContext(transaction, Mock.Of<IServiceProvider>(), CancellationToken.None)
        {
            Destination = "destination",
            Headers =
            {
                {"test_header", "test_header value"}
            },
            Key = Guid.NewGuid().ToByteArray(),
            Value = Guid.NewGuid().ToByteArray()
        };
        await _storage.SaveAsync(produceContext);

        var messages = await _storage.FetchAsync(100, transaction, CancellationToken.None);

        messages.Count.Should().Be(1);

        var message = messages.First();

        message.Destination.Should().Be(produceContext.Destination);
        message.Headers.Should().SatisfyRespectively(header =>
        {
            produceContext.Headers.ContainsKey(header.Key).Should().BeTrue();
            header.Value.Should().Be(produceContext.Headers[header.Key]);
        });
        message.Key.Should().BeEquivalentTo(produceContext.Key);
        message.Value.Should().BeEquivalentTo(produceContext.Value);

        await _storage.DeleteAsync(messages, transaction, CancellationToken.None);

        messages = await _storage.FetchAsync(100, transaction, CancellationToken.None);

        messages.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveWithNullDestinationThrowsTest()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        _transactions.Add(transaction);

        var context = new ProduceContext(transaction, Mock.Of<IServiceProvider>(), CancellationToken.None)
        {
            Destination = null,
            Value = [1, 2, 3]
        };

        var act = async () => await _storage.SaveAsync(context);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Destination must be defined.");
    }

    [Fact]
    public async Task SaveWithEmptyDestinationThrowsTest()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        _transactions.Add(transaction);

        var context = new ProduceContext(transaction, Mock.Of<IServiceProvider>(), CancellationToken.None)
        {
            Destination = string.Empty,
            Value = [1, 2, 3]
        };

        var act = async () => await _storage.SaveAsync(context);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Destination must be defined.");
    }

    [Fact]
    public async Task SaveWithNullValueThrowsTest()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        _transactions.Add(transaction);

        var context = new ProduceContext(transaction, Mock.Of<IServiceProvider>(), CancellationToken.None)
        {
            Destination = "test-destination",
            Value = null
        };

        var act = async () => await _storage.SaveAsync(context);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Value must be defined.");
    }

    [Fact]
    public async Task FetchWithNonPositiveBatchSizeThrowsTest()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        _transactions.Add(transaction);

        Func<Task> actZero = async () => await _storage.FetchAsync(0, transaction);
        await actZero.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("batchSize");

        Func<Task> actNegative = async () => await _storage.FetchAsync(-1, transaction);
        await actNegative.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("batchSize");
    }

    [Fact]
    public async Task MessagesReturnedInOrderTest()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        _transactions.Add(transaction);

        var context1 = new ProduceContext(transaction, Mock.Of<IServiceProvider>(), CancellationToken.None)
        {
            Destination = "order-test",
            Value = [1]
        };
        var context2 = new ProduceContext(transaction, Mock.Of<IServiceProvider>(), CancellationToken.None)
        {
            Destination = "order-test",
            Value = [2]
        };

        await _storage.SaveAsync(context1);
        await _storage.SaveAsync(context2);

        var messages = await _storage.FetchAsync(100, transaction, CancellationToken.None);
        messages.Count.Should().Be(2);
        messages.ElementAt(0).Value[0].Should().Be(1);
        messages.ElementAt(1).Value[0].Should().Be(2);
    }

    [Fact]
    public async Task DeleteWithEmptyListDoesNotThrowTest()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        _transactions.Add(transaction);

        var act = async () => await _storage.DeleteAsync(
            Array.Empty<IOutboxMessage>(), transaction);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task BatchSizeLimitTest()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        _transactions.Add(transaction);

        for (var i = 0; i < 5; i++)
        {
            var context = new ProduceContext(transaction, Mock.Of<IServiceProvider>(), CancellationToken.None)
            {
                Destination = "batch-test",
                Value = [(byte) i]
            };
            await _storage.SaveAsync(context);
        }

        var messages = await _storage.FetchAsync(3, transaction, CancellationToken.None);
        messages.Count.Should().Be(3);
    }
}