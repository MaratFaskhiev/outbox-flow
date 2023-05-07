using Microsoft.Extensions.DependencyInjection;
using OutboxFlow.Abstractions;

namespace OutboxFlow.Postgres;

/// <summary>
/// Extension methods for setting up PostgreSQL as an outbox storage.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds PostgreSQL <see cref="IStorage" /> implementation.
    /// </summary>
    /// <param name="services">Collection of service descriptors.</param>
    public static IServiceCollection UsePostgres(this IServiceCollection services)
    {
        return services.AddScoped<IStorage, Storage>();
    }
}