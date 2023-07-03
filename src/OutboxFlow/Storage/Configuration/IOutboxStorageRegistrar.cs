using Microsoft.Extensions.DependencyInjection;

namespace OutboxFlow.Storage.Configuration;

/// <summary>
/// Registers an outbox storage.
/// </summary>
public interface IOutboxStorageRegistrar
{
    /// <summary>
    /// Registers an outbox storage.
    /// </summary>
    /// <param name="services">Collection of service descriptors.</param>
    public void Register(IServiceCollection services);
}