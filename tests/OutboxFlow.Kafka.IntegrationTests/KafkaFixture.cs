using Testcontainers.Kafka;
using Xunit;

namespace OutboxFlow.Kafka.IntegrationTests;

#pragma warning disable CA1515
public sealed class KafkaFixture : IAsyncLifetime
#pragma warning restore CA1515
{
    private readonly KafkaContainer _kafkaContainer = new KafkaBuilder("apache/kafka:3.9.0")
        .WithKRaft()
        .WithVendor(KafkaVendor.ApacheSoftwareFoundation)
        .Build();

    public string BootstrapAddress => _kafkaContainer.GetBootstrapAddress();

    public Task InitializeAsync()
    {
        return _kafkaContainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _kafkaContainer.DisposeAsync().AsTask();
    }
}