using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OutboxFlow.Kafka;

/// <summary>
/// Extension methods for setting up Apache Kafka as a destination.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Apache Kafka dependencies.
    /// </summary>
    /// <param name="services">Collection of service descriptors.</param>
    public static IServiceCollection AddKafka(this IServiceCollection services)
    {
        services.TryAddSingleton<IKafkaProducerRegistry, KafkaProducerRegistry>();

        return services;
    }
}