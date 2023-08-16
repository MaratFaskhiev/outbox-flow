using Testcontainers.Kafka;
using Xunit;

namespace OutboxFlow.Kafka.IntegrationTests;

public sealed class KafkaFixture : IAsyncLifetime
{
    private readonly KafkaContainer _kafkaContainer = new KafkaBuilder().Build();

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