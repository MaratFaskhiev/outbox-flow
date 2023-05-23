using System.Data;
using Npgsql;
using NpgsqlTypes;
using OutboxFlow.Abstractions;

namespace OutboxFlow.Postgres;

/// <summary>
/// Outbox message storage which uses PostgreSQL as an underlying storage.
/// </summary>
public sealed class OutboxStorage : IOutboxStorage
{
    private const string FetchCommandText = @"
select id, destination, key, value from outbox_message
order by id
limit @batch_size;";

    private const string InsertCommandText = @"
insert into outbox_message (destination, key, value, created_at)
values (@destination, @key, @value, @created_at);";

    private const string DeleteCommandText = @"
delete from outbox_message
where id = any(@ids);";

    /// <inheritdoc />
    public async ValueTask<IReadOnlyCollection<IOutboxMessage>> FetchAsync(
        int batchSize,
        IDbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        if (batchSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size should be greater than zero.");

        var connection = transaction.Connection as NpgsqlConnection;
        if (connection == null)
            throw new InvalidOperationException("Connection must be defined.");

        var command = connection.CreateCommand();
        await using (command.ConfigureAwait(false))
        {
            command.CommandText = FetchCommandText;

            command.Parameters.AddWithValue("batch_size", batchSize);

            var reader = await command
                .ExecuteReaderAsync(CommandBehavior.Default | CommandBehavior.SequentialAccess, cancellationToken)
                .ConfigureAwait(false);
            await using (reader.ConfigureAwait(false))
            {
                var result = new List<IOutboxMessage>(batchSize);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var id = await reader.GetFieldValueAsync<long>(0, cancellationToken).ConfigureAwait(false);

                    string? destination = null;
                    if (!await reader.IsDBNullAsync(1, cancellationToken).ConfigureAwait(false))
                        destination = await reader.GetFieldValueAsync<string>(1, cancellationToken)
                            .ConfigureAwait(false);

                    byte[]? key = null;
                    if (!await reader.IsDBNullAsync(2, cancellationToken).ConfigureAwait(false))
                        key = await reader.GetFieldValueAsync<byte[]>(2, cancellationToken)
                            .ConfigureAwait(false);

                    var message = new OutboxMessage(
                        id,
                        destination,
                        key,
                        await reader.GetFieldValueAsync<byte[]>(3, cancellationToken).ConfigureAwait(false));

                    result.Add(message);
                }

                return result;
            }
        }
    }

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

            command.Parameters.AddWithValue("destination", (object?) context.Destination ?? DBNull.Value);
            command.Parameters.AddWithValue("key", NpgsqlDbType.Bytea, (object?) context.Key ?? DBNull.Value);
            command.Parameters.AddWithValue("value", context.Value);
            command.Parameters.AddWithValue("created_at", DateTime.UtcNow);

            await command.ExecuteNonQueryAsync(context.CancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask DeleteAsync(IReadOnlyCollection<IOutboxMessage> outboxMessages, IDbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        var connection = transaction.Connection as NpgsqlConnection;
        if (connection == null)
            throw new InvalidOperationException("Connection must be defined.");

        if (outboxMessages.Any(x => x is not OutboxMessage))
            throw new InvalidOperationException($"{typeof(OutboxMessage).FullName} expected.");

        var command = connection.CreateCommand();
        await using (command.ConfigureAwait(false))
        {
            command.CommandText = DeleteCommandText;

            command.Parameters.AddWithValue("@ids", outboxMessages.Select(x => ((OutboxMessage) x).Id).ToArray());

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}