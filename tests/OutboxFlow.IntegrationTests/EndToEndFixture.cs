using Testcontainers.Kafka;
using Testcontainers.PostgreSql;
using Xunit;

namespace OutboxFlow.IntegrationTests;

public sealed class EndToEndFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:15-alpine")
        .WithResourceMapping("postgres.sql", "/docker-entrypoint-initdb.d")
        .WithReuse(true)
        .Build();

    private readonly KafkaContainer _kafka = new KafkaBuilder("confluentinc/cp-kafka:7.7.1")
        .WithReuse(true)
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