using Npgsql;
using NpgsqlTypes;
using OutboxFlow.Abstractions;

namespace OutboxFlow.Postgres;

/// <summary>
/// Outbox message storage which uses PostgreSQL as an underlying storage.
/// </summary>
public sealed class Storage : IStorage
{
    private const string InsertCommandText = @"
insert into outbox_message (destination, ""key"", ""value"", created_at)
values (@destination, @key, @value, @created_at);";

    /// <inheritdoc />
    public async ValueTask SaveAsync(IProduceContext context)
    {
        if (string.IsNullOrEmpty(context.Destination))
            throw new InvalidOperationException("Destination must be defined.");

        if (context.Value == null) throw new InvalidOperationException("Value must be defined.");

        var connection = context.Transaction.Connection as NpgsqlConnection;
        if (connection == null)
            throw new InvalidOperationException("Connection must be defined.");

        var command = connection.CreateCommand();
        await using (command.ConfigureAwait(false))
        {
            command.CommandText = InsertCommandText;

            command.Parameters.AddWithValue("destination", context.Destination);
            command.Parameters.AddWithValue("key", NpgsqlDbType.Bytea, (object?)context.Key ?? DBNull.Value);
            command.Parameters.AddWithValue("value", context.Value);
            command.Parameters.AddWithValue("created_at", DateTime.UtcNow);

            await command.ExecuteNonQueryAsync(context.CancellationToken).ConfigureAwait(false);
        }
    }
}