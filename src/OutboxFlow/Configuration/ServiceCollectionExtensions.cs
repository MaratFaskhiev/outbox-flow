using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OutboxFlow.Configuration;

/// <summary>
/// Extension method for setting up an outbox.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the outbox dependencies.
    /// </summary>
    /// <param name="services">Collection of service descriptors.</param>
    /// <param name="configure">Outbox configure action.</param>
    public static IServiceCollection AddOutbox(this IServiceCollection services, Action<IOutboxBuilder> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        services.TryAddSingleton<IClock, Clock>();

        var builder = new OutboxBuilder();
        configure(builder);
        builder.Build(services);

        return services;
    }
}