using System.Data;
using Npgsql;
using OutboxFlow.Storage;

namespace OutboxFlow.Postgres;

/// <summary>
/// Outbox lock manager which uses PostgreSQL as an underlying storage.
/// </summary>
public sealed class OutboxLockManager : IOutboxLockManager
{
    private const long LockKey = 0x4F_55_54_42_4F_58;

    private const string TryLockCommandText =
        "SELECT pg_try_advisory_xact_lock(@lock_key);";

    private const string CheckLockCommandText =
        "SELECT 1 FROM outbox_state WHERE expire_at >= clock_timestamp();";

    private const string InsertCommandText = @"
insert into outbox_state (expire_at) values (clock_timestamp() + @timeout)
returning id, expire_at;
";

    private const string ReleaseCommandText = @"
delete from outbox_state
where expire_at < clock_timestamp() or id = @id;
";

    private readonly NpgsqlConnection _connection;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="connection">Database connection.</param>
    public OutboxLockManager(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async ValueTask<IOutboxLock?> LockAsync(
        TimeSpan lockTimeout, CancellationToken cancellationToken = default)
    {
        await EnsureOpenAsync(cancellationToken).ConfigureAwait(false);

        using var checkCommand = _connection.CreateCommand();
        checkCommand.CommandText = CheckLockCommandText;
        var existingLock = await checkCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (existingLock != null) return null;

        using var tryLockCommand = _connection.CreateCommand();
        tryLockCommand.CommandText = TryLockCommandText;
        tryLockCommand.Parameters.AddWithValue("@lock_key", LockKey);

        var lockAcquired = (bool?) await tryLockCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (lockAcquired != true) return null;

        using var insertCommand = _connection.CreateCommand();
        insertCommand.CommandText = InsertCommandText;
        insertCommand.Parameters.AddWithValue("@timeout", lockTimeout);

        var reader = await insertCommand
            .ExecuteReaderAsync(CommandBehavior.Default | CommandBehavior.SequentialAccess, cancellationToken)
            .ConfigureAwait(false);
        await using (reader.ConfigureAwait(false))
        {
            await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            var id = await reader.GetFieldValueAsync<Guid>(0, cancellationToken).ConfigureAwait(false);
            var expireAt = await reader.GetFieldValueAsync<DateTime>(1, cancellationToken).ConfigureAwait(false);

            return new OutboxLock(id, expireAt);
        }
    }

    /// <inheritdoc />
    public async ValueTask ReleaseAsync(IOutboxLock outboxLock,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(outboxLock);

        await EnsureOpenAsync(cancellationToken).ConfigureAwait(false);

        using var command = _connection.CreateCommand();
        command.CommandText = ReleaseCommandText;
        command.Parameters.AddWithValue("@id", outboxLock.Id);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask EnsureOpenAsync(CancellationToken ct)
    {
        if (_connection.State == ConnectionState.Closed)
            await _connection.OpenAsync(ct).ConfigureAwait(false);
    }
}