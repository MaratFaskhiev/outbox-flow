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

    private readonly IDbConnectionFactory _connectionFactory;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="connectionFactory">Database connection factory.</param>
    public OutboxLockManager(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc />
    public async ValueTask<IOutboxLock?> LockAsync(
        TimeSpan lockTimeout, CancellationToken cancellationToken = default)
    {
        await using var connection = await GetConnectionAsync(cancellationToken);

        using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = CheckLockCommandText;
        var existingLock = await checkCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (existingLock != null) return null;

        using var tryLockCommand = connection.CreateCommand();
        tryLockCommand.CommandText = TryLockCommandText;
        tryLockCommand.Parameters.AddWithValue("@lock_key", LockKey);

        var lockAcquired = (bool?) await tryLockCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (lockAcquired != true) return null;

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
    public async ValueTask ReleaseAsync(IOutboxLock outboxLock,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await GetConnectionAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = ReleaseCommandText;
        command.Parameters.AddWithValue("@id", outboxLock.Id);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<NpgsqlConnection> GetConnectionAsync(CancellationToken ct)
    {
        var connection = (NpgsqlConnection) _connectionFactory.Create();
        await connection.OpenAsync(ct);
        return connection;
    }
}