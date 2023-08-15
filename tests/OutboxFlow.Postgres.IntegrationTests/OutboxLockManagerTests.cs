using Npgsql;
using Xunit;

namespace OutboxFlow.Postgres.IntegrationTests;

public sealed class OutboxLockManagerTests : IClassFixture<DatabaseFixture>
{
    private readonly string _connectionString;

    private readonly OutboxLockManager _manager = new();

    public OutboxLockManagerTests(DatabaseFixture databaseFixture)
    {
        _connectionString = databaseFixture.ConnectionString;
    }

    [Fact]
    public async Task LockAsync_NoLock_ReturnsLockRecord()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var outboxLock = await _manager.LockAsync(TimeSpan.FromMinutes(5), transaction, CancellationToken.None);

        Assert.NotNull(outboxLock);
    }
}