using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace OutboxFlow.Kafka.UnitTests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddKafka_RegistersKafkaProducerRegistry()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ILogger<DefaultKafkaProducerBuilder>>(
            NullLogger<DefaultKafkaProducerBuilder>.Instance);
        services.AddKafka();

        var sp = services.BuildServiceProvider();
        var registry = sp.GetRequiredService<IKafkaProducerRegistry>();
        registry.Should().BeOfType<KafkaProducerRegistry>();
    }

    [Fact]
    public void AddKafka_IKafkaProducerBuilder_ResolvesToSameInstanceAsDefaultKafkaProducerBuilder()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ILogger<DefaultKafkaProducerBuilder>>(
            NullLogger<DefaultKafkaProducerBuilder>.Instance);
        services.AddKafka();

        var sp = services.BuildServiceProvider();
        var asInterface = sp.GetRequiredService<IKafkaProducerBuilder>();
        var asConcrete = sp.GetRequiredService<DefaultKafkaProducerBuilder>();

        asInterface.Should().BeSameAs(asConcrete);
    }
}