using Xunit;

namespace OutboxFlow.Postgres.UnitTests;

public sealed class DefaultDbConnectionFactoryTests
{
    private readonly DefaultDbConnectionFactory _factory = new(string.Empty);

    [Fact]
    public void Create_ReturnsConnection()
    {
        var connection = _factory.Create();

        Assert.NotNull(connection);
    }
}