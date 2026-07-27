using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using OutboxFlow.Storage;
using OutboxFlow.Storage.Configuration;

namespace OutboxFlow.Postgres;

/// <summary>
/// Registers an outbox storage based on PostgreSQL.
/// </summary>
public sealed class ProducerOutboxStorageRegistrar : IOutboxStorageRegistrar
{
    private readonly string _connectionString;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="connectionString">Database connection string.</param>
    public ProducerOutboxStorageRegistrar(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Registers an outbox storage based on PostgreSQL.
    /// </summary>
    /// <param name="services">Collection of service descriptors.</param>
    public void Register(IServiceCollection services)
    {
        services.TryAddScoped<IOutboxStorage, OutboxStorage>();
        services.TryAddSingleton<IDbConnectionFactory>(new DefaultDbConnectionFactory(_connectionString));
        services.TryAddScoped<NpgsqlConnection>(sp =>
            (NpgsqlConnection) sp.GetRequiredService<IDbConnectionFactory>().Create());
        services.TryAddScoped<IDbConnection>(sp =>
            sp.GetRequiredService<NpgsqlConnection>());
    }
}