using Testcontainers.Kafka;
using Testcontainers.PostgreSql;
using Xunit;

namespace OutboxFlow.IntegrationTests;

public sealed class EndToEndFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17")
        .WithResourceMapping("postgres.sql", "/docker-entrypoint-initdb.d")
        .Build();

    private readonly KafkaContainer _kafka = new KafkaBuilder("apache/kafka:3.9.0")
        .WithKRaft()
        .WithVendor(KafkaVendor.ApacheSoftwareFoundation)
        .Build();

    public string ConnectionString => _postgres.GetConnectionString();
    public string BootstrapAddress => _kafka.GetBootstrapAddress();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _kafka.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync().AsTask();
        await _kafka.DisposeAsync().AsTask();
    }
}