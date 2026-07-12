using Xunit;

namespace OutboxFlow.Postgres.IntegrationTests;

[CollectionDefinition("PostgresIntegration", DisableParallelization = true)]
public sealed class PostgresIntegrationCollection : ICollectionFixture<DatabaseFixture>
{
}