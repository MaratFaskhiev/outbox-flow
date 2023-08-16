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
    public async Task LockWorkflowTest()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var outboxLock1 = await _manager.LockAsync(TimeSpan.FromMinutes(5), transaction, CancellationToken.None);

        Assert.NotNull(outboxLock1);

        var outboxLock2 = await _manager.LockAsync(TimeSpan.FromMinutes(5), transaction, CancellationToken.None);

        Assert.Null(outboxLock2);

        await _manager.ReleaseAsync(outboxLock1, transaction, CancellationToken.None);

        var outboxLock3 = await _manager.LockAsync(TimeSpan.FromMinutes(5), transaction, CancellationToken.None);

        Assert.NotNull(outboxLock3);
    }
}