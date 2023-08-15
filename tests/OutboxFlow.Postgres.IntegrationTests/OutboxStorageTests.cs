using Moq;
using Npgsql;
using OutboxFlow.Produce;
using Xunit;

namespace OutboxFlow.Postgres.IntegrationTests;

public sealed class OutboxStorageTests : IClassFixture<DatabaseFixture>
{
    private readonly string _connectionString;

    private readonly OutboxStorage _storage = new();

    public OutboxStorageTests(DatabaseFixture databaseFixture)
    {
        _connectionString = databaseFixture.ConnectionString;
    }

    [Fact]
    public async Task MessageWorkflowTest()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var produceContext = new ProduceContext(transaction, Mock.Of<IServiceProvider>(), CancellationToken.None)
        {
            Destination = "destination",
            Key = Guid.NewGuid().ToByteArray(),
            Value = Guid.NewGuid().ToByteArray()
        };
        await _storage.SaveAsync(produceContext);

        var messages = await _storage.FetchAsync(100, transaction, CancellationToken.None);

        Assert.Equal(1, messages.Count);

        var message = messages.First();

        Assert.Equal(produceContext.Destination, message.Destination);
        Assert.Equal(produceContext.Key, message.Key);
        Assert.Equal(produceContext.Value, message.Value);

        await _storage.DeleteAsync(messages, transaction, CancellationToken.None);

        messages = await _storage.FetchAsync(100, transaction, CancellationToken.None);

        Assert.Equal(0, messages.Count);
    }
}