using Microsoft.Extensions.DependencyInjection;

namespace OutboxFlow.Configuration;

/// <summary>
/// Extension method for setting up an outbox.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds an outbox.
    /// </summary>
    /// <param name="services">Collection of service descriptors.</param>
    /// <param name="configure">Outbox configure action.</param>
    public static IServiceCollection AddOutbox(this IServiceCollection services, Action<IOutboxBuilder> configure)
    {
        var builder = new OutboxBuilder();
        configure(builder);
        builder.Build(services);

        return services;
    }
}