using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OutboxFlow.Abstractions;
using OutboxFlow.Configuration;

namespace OutboxFlow.Postgres;

/// <summary>
/// Registers an outbox storage based on PostgreSQL.
/// </summary>
public sealed class ConsumerOutboxStorageRegistrar : IOutboxStorageRegistrar
{
    private readonly string _connectionString;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="connectionString">Database connection string.</param>
    public ConsumerOutboxStorageRegistrar(string connectionString)
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
    }
}