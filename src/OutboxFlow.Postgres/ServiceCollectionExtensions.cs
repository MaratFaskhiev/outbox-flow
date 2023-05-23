using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OutboxFlow.Abstractions;

namespace OutboxFlow.Postgres;

/// <summary>
/// Extension methods for setting up PostgreSQL as an outbox storage.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds PostgreSQL <see cref="IOutboxStorage" /> implementation.
    /// </summary>
    /// <param name="services">Collection of service descriptors.</param>
    /// <param name="connectionString">Connection string.</param>
    public static IServiceCollection UsePostgres(this IServiceCollection services, string connectionString)
    {
        services.TryAddSingleton<IDbConnectionFactory>(new DefaultDbConnectionFactory(connectionString));
        services.TryAddScoped<IOutboxStorage, OutboxStorage>();

        return services;
    }
}