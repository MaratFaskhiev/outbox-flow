using FluentAssertions;
using Npgsql;
using Xunit;

namespace OutboxFlow.Postgres.IntegrationTests;

[Collection("PostgresIntegration")]
[Trait("Category", "Integration")]
public sealed class OutboxLockManagerTests
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

        outboxLock1.Should().NotBeNull();

        var outboxLock2 = await _manager.LockAsync(TimeSpan.FromMinutes(5), transaction, CancellationToken.None);

        outboxLock2.Should().BeNull();

        await _manager.ReleaseAsync(outboxLock1, transaction, CancellationToken.None);

        var outboxLock3 = await _manager.LockAsync(TimeSpan.FromMinutes(5), transaction, CancellationToken.None);

        outboxLock3.Should().NotBeNull();
    }

    [Fact]
    public async Task ConcurrentLockFromTwoConnectionsTest()
    {
        await using var connection1 = new NpgsqlConnection(_connectionString);
        await connection1.OpenAsync();
        await using var transaction1 = await connection1.BeginTransactionAsync();

        var lock1 = await _manager.LockAsync(TimeSpan.FromMinutes(5), transaction1, CancellationToken.None);
        lock1.Should().NotBeNull();

        await using var connection2 = new NpgsqlConnection(_connectionString);
        await connection2.OpenAsync();
        var transaction2 = await connection2.BeginTransactionAsync();

        var lock2 = await _manager.LockAsync(TimeSpan.FromMinutes(5), transaction2, CancellationToken.None);
        lock2.Should().BeNull();

        // transaction2 is aborted after the LOCK TABLE failure; rollback before creating a new one
        await transaction2.RollbackAsync();
        await transaction2.DisposeAsync();

        await _manager.ReleaseAsync(lock1, transaction1, CancellationToken.None);
        await transaction1.CommitAsync();

        await using var transaction3 = await connection2.BeginTransactionAsync();
        var lock3 = await _manager.LockAsync(TimeSpan.FromMinutes(5), transaction3, CancellationToken.None);
        lock3.Should().NotBeNull();

        await _manager.ReleaseAsync(lock3, transaction3, CancellationToken.None);
        await transaction3.CommitAsync();
    }

    [Fact]
    public async Task LockExpirationTest()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var lock1 = await _manager.LockAsync(TimeSpan.FromSeconds(1), transaction, CancellationToken.None);
        lock1.Should().NotBeNull();

        await Task.Delay(TimeSpan.FromSeconds(2));

        var lock2 = await _manager.LockAsync(TimeSpan.FromMinutes(5), transaction, CancellationToken.None);
        lock2.Should().NotBeNull();

        await _manager.ReleaseAsync(lock2, transaction, CancellationToken.None);
        await transaction.CommitAsync();
    }

    [Fact]
    public async Task CancellationTest()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = async () => await _manager.LockAsync(
            TimeSpan.FromMinutes(5), transaction, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task DoubleReleaseTest()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var outboxLock = await _manager.LockAsync(TimeSpan.FromMinutes(5), transaction, CancellationToken.None);
        outboxLock.Should().NotBeNull();

        await _manager.ReleaseAsync(outboxLock, transaction, CancellationToken.None);

        var act = async () => await _manager.ReleaseAsync(outboxLock, transaction, CancellationToken.None);
        await act.Should().NotThrowAsync();

        await transaction.CommitAsync();
    }
}