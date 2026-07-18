using System.Transactions;
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
    private readonly OutboxStorage _storage;

    public OutboxStorageTests(DatabaseFixture databaseFixture)
    {
        _connectionString = databaseFixture.ConnectionString;
        _storage = new OutboxStorage(new DefaultDbConnectionFactory(_connectionString));
    }

    public async Task InitializeAsync()
    {
        await CleanupAsync();
    }

    public async Task DisposeAsync()
    {
        await CleanupAsync();
    }

    private async Task CleanupAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "delete from outbox_message; delete from outbox_state;";
        await cmd.ExecuteNonQueryAsync();
    }

    [Fact]
    public async Task MessageWorkflowTest()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var produceContext = new ProduceContext(Mock.Of<IServiceProvider>(), CancellationToken.None)
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

        var messages = await _storage.FetchAsync(100, CancellationToken.None);

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

        await _storage.DeleteAsync(messages, CancellationToken.None);

        messages = await _storage.FetchAsync(100, CancellationToken.None);

        messages.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveWithNullDestinationThrowsTest()
    {
        var context = new ProduceContext(Mock.Of<IServiceProvider>(), CancellationToken.None)
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
        var context = new ProduceContext(Mock.Of<IServiceProvider>(), CancellationToken.None)
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
        var context = new ProduceContext(Mock.Of<IServiceProvider>(), CancellationToken.None)
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
        Func<Task> actZero = async () => await _storage.FetchAsync(0);
        await actZero.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("batchSize");

        Func<Task> actNegative = async () => await _storage.FetchAsync(-1);
        await actNegative.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("batchSize");
    }

    [Fact]
    public async Task MessagesReturnedInOrderTest()
    {
        var context1 = new ProduceContext(Mock.Of<IServiceProvider>(), CancellationToken.None)
        {
            Destination = "order-test",
            Value = [1]
        };
        var context2 = new ProduceContext(Mock.Of<IServiceProvider>(), CancellationToken.None)
        {
            Destination = "order-test",
            Value = [2]
        };

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await _storage.SaveAsync(context1);
            await _storage.SaveAsync(context2);
            scope.Complete();
        }

        var messages = await _storage.FetchAsync(100, CancellationToken.None);
        messages.Count.Should().Be(2);
        messages.ElementAt(0).Value[0].Should().Be(1);
        messages.ElementAt(1).Value[0].Should().Be(2);
    }

    [Fact]
    public async Task DeleteWithEmptyListDoesNotThrowTest()
    {
        var act = async () => await _storage.DeleteAsync(
            Array.Empty<IOutboxMessage>());

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task BatchSizeLimitTest()
    {
        for (var i = 0; i < 5; i++)
        {
            var context = new ProduceContext(Mock.Of<IServiceProvider>(), CancellationToken.None)
            {
                Destination = "batch-test",
                Value = [(byte) i]
            };
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _storage.SaveAsync(context);
                scope.Complete();
            }
        }

        var messages = await _storage.FetchAsync(3, CancellationToken.None);
        messages.Count.Should().Be(3);
    }

    [Fact]
    public async Task SaveBatchAsync_SavesAllMessages()
    {
        var contexts = new List<IProduceContext>
        {
            new ProduceContext(Mock.Of<IServiceProvider>(), CancellationToken.None)
            {
                Destination = "batch-test", Value = [1]
            },
            new ProduceContext(Mock.Of<IServiceProvider>(), CancellationToken.None)
            {
                Destination = "batch-test", Value = [2]
            },
            new ProduceContext(Mock.Of<IServiceProvider>(), CancellationToken.None)
            {
                Destination = "batch-test", Value = [3]
            }
        };

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await _storage.SaveBatchAsync(contexts);
            scope.Complete();
        }

        var messages = await _storage.FetchAsync(100, CancellationToken.None);
        messages.Count.Should().Be(3);
        messages.Select(m => m.Value[0]).Should().BeEquivalentTo(new byte[] {1, 2, 3});
    }

    [Fact]
    public async Task SaveBatchAsync_NullDestination_Throws()
    {
        var contexts = new List<IProduceContext>
        {
            new ProduceContext(Mock.Of<IServiceProvider>(), CancellationToken.None)
            {
                Destination = "valid", Value = [1]
            },
            new ProduceContext(Mock.Of<IServiceProvider>(), CancellationToken.None)
            {
                Destination = null, Value = [2]
            }
        };

        var act = async () => await _storage.SaveBatchAsync(contexts);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Destination must be defined.");
    }

    [Fact]
    public async Task SaveBatchAsync_NullValue_Throws()
    {
        var contexts = new List<IProduceContext>
        {
            new ProduceContext(Mock.Of<IServiceProvider>(), CancellationToken.None)
            {
                Destination = "valid", Value = [1]
            },
            new ProduceContext(Mock.Of<IServiceProvider>(), CancellationToken.None)
            {
                Destination = "valid", Value = null
            }
        };

        var act = async () => await _storage.SaveBatchAsync(contexts);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Value must be defined.");
    }

    [Fact]
    public async Task SaveBatchAsync_EmptyBatch_DoesNotThrow()
    {
        var act = async () => await _storage.SaveBatchAsync(
            Array.Empty<IProduceContext>());

        await act.Should().NotThrowAsync();
    }
}