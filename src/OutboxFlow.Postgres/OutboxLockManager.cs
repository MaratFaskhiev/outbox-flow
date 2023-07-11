using System.Data;
using Npgsql;
using OutboxFlow.Storage;

namespace OutboxFlow.Postgres;

/// <summary>
/// Outbox lock manager which uses PostgreSQL as an underlying storage.
/// </summary>
public sealed class OutboxLockManager : IOutboxLockManager
{
    private const string CountCommandText = @"
lock table outbox_state in access exclusive mode nowait;

select count(id) from outbox_state
where expire_at > now();
";

    private const string InsertCommandText = @"
insert into outbox_state (expire_at) values (now() + @timeout)
returning id, expire_at;
";

    private const string ReleaseCommandText = @"
delete from outbox_state
where expire_at < now() or id = @id;
";

    private const string LockNotAvailableCode = "55P03";

    /// <inheritdoc />
    public async ValueTask<IOutboxLock?> LockAsync(
        TimeSpan lockTimeout, IDbTransaction transaction, CancellationToken cancellationToken = default)
    {
        var connection = EnsureConnection(transaction);

        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = CountCommandText;
        try
        {
            var count = (long?) await countCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            if (count > 0) return null;
        }
        catch (PostgresException exception) when (exception.SqlState == LockNotAvailableCode)
        {
            return null;
        }

        using var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = InsertCommandText;
        insertCommand.Parameters.AddWithValue("@timeout", lockTimeout);

        await using var reader = await insertCommand
            .ExecuteReaderAsync(CommandBehavior.Default | CommandBehavior.SequentialAccess, cancellationToken)
            .ConfigureAwait(false);
        await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        var id = await reader.GetFieldValueAsync<Guid>(0, cancellationToken).ConfigureAwait(false);
        var expireAt = await reader.GetFieldValueAsync<DateTime>(1, cancellationToken).ConfigureAwait(false);

        return new OutboxLock(id, expireAt);
    }

    /// <inheritdoc />
    public async ValueTask ReleaseAsync(IOutboxLock outboxLock, IDbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        var connection = EnsureConnection(transaction);

        using var command = connection.CreateCommand();
        command.CommandText = ReleaseCommandText;
        command.Parameters.AddWithValue("@id", outboxLock.Id);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static NpgsqlConnection EnsureConnection(IDbTransaction transaction)
    {
        var connection = transaction.Connection as NpgsqlConnection;
        if (connection == null)
            throw new InvalidOperationException("Connection must be defined.");
        return connection;
    }
}