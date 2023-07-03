using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OutboxFlow.Storage;
using OutboxFlow.Storage.Configuration;

namespace OutboxFlow.Postgres;

/// <summary>
/// Registers an outbox storage based on PostgreSQL.
/// </summary>
public sealed class ProducerOutboxStorageRegistrar : IOutboxStorageRegistrar
{
    /// <summary>
    /// Registers an outbox storage based on PostgreSQL.
    /// </summary>
    /// <param name="services">Collection of service descriptors.</param>
    public void Register(IServiceCollection services)
    {
        services.TryAddScoped<IOutboxStorage, OutboxStorage>();
    }
}