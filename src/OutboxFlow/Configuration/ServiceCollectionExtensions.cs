using Microsoft.Extensions.DependencyInjection;

namespace OutboxFlow.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds an outbox.
    /// </summary>
    /// <param name="services">Collection of service descriptors.</param>
    /// <param name="configure">Outbox configure action.</param>
    public static IServiceCollection AddOutbox(this IServiceCollection services, Action<Builder> configure)
    {
        var builder = new Builder();
        configure(builder);
        builder.Build(services);

        return services;
    }
}