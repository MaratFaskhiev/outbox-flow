using Testcontainers.PostgreSql;
using Xunit;

namespace OutboxFlow.Postgres.IntegrationTests;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithResourceMapping("postgres.sql", "/docker-entrypoint-initdb.d")
        .Build();

    public string ConnectionString => _postgreSqlContainer.GetConnectionString();

    public Task InitializeAsync()
    {
        return _postgreSqlContainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _postgreSqlContainer.DisposeAsync().AsTask();
    }
}