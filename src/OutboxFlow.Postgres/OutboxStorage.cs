using System.Data;
using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using OutboxFlow.Produce;
using OutboxFlow.Storage;

namespace OutboxFlow.Postgres;

/// <summary>
/// Outbox message storage which uses PostgreSQL as an underlying storage.
/// </summary>
public sealed class OutboxStorage : IOutboxStorage
{
    private const string FetchCommandText = @"
select id, destination, headers, key, value from outbox_message
order by id
limit @batch_size;";

    private const string InsertCommandText = @"
insert into outbox_message (destination, headers, key, value, created_at)
values (@destination, @headers::jsonb, @key, @value, @created_at);";

    private const string DeleteCommandText = @"
delete from outbox_message
where id = any(@ids);";

    private readonly IDbConnectionFactory _connectionFactory;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="connectionFactory">Database connection factory.</param>
    public OutboxStorage(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyCollection<IOutboxMessage>> FetchAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        if (batchSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size should be greater than zero.");

        var connection = await GetConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            using var command = connection.CreateCommand();
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

                    string? headers = null;
                    if (!await reader.IsDBNullAsync(2, cancellationToken).ConfigureAwait(false))
                        headers = await reader.GetFieldValueAsync<string>(2, cancellationToken)
                            .ConfigureAwait(false);

                    byte[]? key = null;
                    if (!await reader.IsDBNullAsync(3, cancellationToken).ConfigureAwait(false))
                        key = await reader.GetFieldValueAsync<byte[]>(3, cancellationToken)
                            .ConfigureAwait(false);

                    var value = await reader.GetFieldValueAsync<byte[]>(4, cancellationToken).ConfigureAwait(false);

                    var message = new OutboxMessage(
                        id,
                        destination,
                        DeserializeHeaders(headers),
                        key,
                        value);

                    result.Add(message);
                }

                return result;
            }
        }
    }

    /// <inheritdoc />
    public async ValueTask SaveAsync(IProduceContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.EnsureValid();

        var connection = await GetConnectionAsync(context.CancellationToken).ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            using var command = connection.CreateCommand();
            command.CommandText = InsertCommandText;

            command.Parameters.AddWithValue("destination", (object?) context.Destination ?? DBNull.Value);
            command.Parameters.AddWithValue("headers", (object?) SerializeHeaders(context.Headers) ?? DBNull.Value);
            command.Parameters.AddWithValue("key", NpgsqlDbType.Bytea, (object?) context.Key ?? DBNull.Value);
            command.Parameters.AddWithValue("value", (object?) context.Value ?? DBNull.Value);
            command.Parameters.AddWithValue("created_at", DateTime.UtcNow);

            await command.ExecuteNonQueryAsync(context.CancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask SaveBatchAsync(
        IReadOnlyCollection<IProduceContext> contexts)
    {
        ArgumentNullException.ThrowIfNull(contexts);
        if (contexts.Count == 0) return;

        var connection = await GetConnectionAsync(contexts.First().CancellationToken).ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var batch = new NpgsqlBatch(connection);
            await using (batch.ConfigureAwait(false))
            {
                foreach (var ctx in contexts)
                {
                    ctx.EnsureValid();

                    var cmd = new NpgsqlBatchCommand(InsertCommandText);
                    cmd.Parameters.AddWithValue("destination", (object?) ctx.Destination ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("headers", (object?) SerializeHeaders(ctx.Headers) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("key", NpgsqlDbType.Bytea, (object?) ctx.Key ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("value", (object?) ctx.Value ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("created_at", DateTime.UtcNow);
                    batch.BatchCommands.Add(cmd);
                }

                await batch.ExecuteNonQueryAsync(contexts.First().CancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    public async ValueTask DeleteAsync(IReadOnlyCollection<IOutboxMessage> outboxMessages,
        CancellationToken cancellationToken = default)
    {
        var connection = await GetConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            if (outboxMessages.Any(x => x is not OutboxMessage))
                throw new InvalidOperationException($"{typeof(OutboxMessage).FullName} expected.");

            using var command = connection.CreateCommand();
            command.CommandText = DeleteCommandText;

            command.Parameters.AddWithValue("@ids", outboxMessages.Select(x => ((OutboxMessage) x).Id).ToArray());

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async ValueTask<NpgsqlConnection> GetConnectionAsync(CancellationToken ct)
    {
        var connection = (NpgsqlConnection) _connectionFactory.Create();
        await connection.OpenAsync(ct).ConfigureAwait(false);
        return connection;
    }

    private static string? SerializeHeaders(IDictionary<string, string>? headers)
    {
        if (headers == null || !headers.Any()) return null;

        return JsonSerializer.Serialize(headers);
    }

    private static Dictionary<string, string> DeserializeHeaders(string? headers)
    {
        if (headers == null) return new Dictionary<string, string>();

        return JsonSerializer.Deserialize<Dictionary<string, string>>(headers)
               ?? new Dictionary<string, string>();
    }
}