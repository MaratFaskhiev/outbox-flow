using System.Data;
using Npgsql;
using OutboxFlow.Storage;

namespace OutboxFlow.Postgres;

/// <summary>
/// <see cref="IDbConnectionFactory" /> implementation which uses static connection string.
/// </summary>
public sealed class DefaultDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="connectionString">Connection string.</param>
    public DefaultDbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public IDbConnection Create()
    {
        return new NpgsqlConnection(_connectionString);
    }
}