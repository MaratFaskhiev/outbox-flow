using System.Transactions;
using FluentAssertions;
using OutboxFlow.Storage;
using Xunit;

namespace OutboxFlow.Postgres.IntegrationTests;

[Collection("PostgresIntegration")]
[Trait("Category", "Integration")]
public sealed class OutboxLockManagerTests
{
    private readonly string _connectionString;
    private readonly OutboxLockManager _manager;

    public OutboxLockManagerTests(DatabaseFixture databaseFixture)
    {
        _connectionString = databaseFixture.ConnectionString;
        _manager = new OutboxLockManager(new DefaultDbConnectionFactory(_connectionString));
    }

    [Fact]
    public async Task LockWorkflowTest()
    {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var outboxLock1 = await _manager.LockAsync(TimeSpan.FromMinutes(5), CancellationToken.None);
            outboxLock1.Should().NotBeNull();

            var outboxLock2 = await _manager.LockAsync(TimeSpan.FromMinutes(5), CancellationToken.None);
            outboxLock2.Should().BeNull();

            await _manager.ReleaseAsync(outboxLock1, CancellationToken.None);
            scope.Complete();
        }

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var outboxLock3 = await _manager.LockAsync(TimeSpan.FromMinutes(5), CancellationToken.None);
            outboxLock3.Should().NotBeNull();
            scope.Complete();
        }
    }

    [Fact]
    public async Task ConcurrentLockFromTwoConnectionsTest()
    {
        IOutboxLock? lock1;
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            lock1 = await _manager.LockAsync(TimeSpan.FromMinutes(5), CancellationToken.None);
            lock1.Should().NotBeNull();
            scope.Complete();
        }

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var lock2 = await _manager.LockAsync(TimeSpan.FromMinutes(5), CancellationToken.None);
            lock2.Should().BeNull();
            scope.Complete();
        }

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await _manager.ReleaseAsync(lock1, CancellationToken.None);
            var lock3 = await _manager.LockAsync(TimeSpan.FromMinutes(5), CancellationToken.None);
            lock3.Should().NotBeNull();
            await _manager.ReleaseAsync(lock3, CancellationToken.None);
            scope.Complete();
        }
    }

    [Fact]
    public async Task LockExpirationTest()
    {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var lock1 = await _manager.LockAsync(TimeSpan.FromSeconds(1), CancellationToken.None);
            lock1.Should().NotBeNull();
            await _manager.ReleaseAsync(lock1, CancellationToken.None);
            scope.Complete();
        }

        await Task.Delay(TimeSpan.FromSeconds(2));

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var lock2 = await _manager.LockAsync(TimeSpan.FromMinutes(5), CancellationToken.None);
            lock2.Should().NotBeNull();
            await _manager.ReleaseAsync(lock2, CancellationToken.None);
            scope.Complete();
        }
    }

    [Fact]
    public async Task CancellationTest()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = async () => await _manager.LockAsync(
            TimeSpan.FromMinutes(5), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task DoubleReleaseTest()
    {
        IOutboxLock? outboxLock;
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            outboxLock = await _manager.LockAsync(TimeSpan.FromMinutes(5), CancellationToken.None);
            outboxLock.Should().NotBeNull();

            await _manager.ReleaseAsync(outboxLock, CancellationToken.None);

            var act = async () => await _manager.ReleaseAsync(outboxLock, CancellationToken.None);
            await act.Should().NotThrowAsync();

            scope.Complete();
        }
    }
}