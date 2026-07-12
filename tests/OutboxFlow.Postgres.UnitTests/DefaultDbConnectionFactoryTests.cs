using FluentAssertions;
using Npgsql;
using Xunit;

namespace OutboxFlow.Postgres.UnitTests;

public sealed class DefaultDbConnectionFactoryTests
{
    private readonly DefaultDbConnectionFactory _factory = new(string.Empty);

    [Fact]
    public void Create_ReturnsConnection()
    {
        var connection = _factory.Create();

        connection.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_InvalidConnectionString_ThrowsException()
    {
        var factory =
            new DefaultDbConnectionFactory("Host=invalid_host;Database=nonexistent;Username=bad;Password=wrong");
        var connection = factory.Create();

        var action = async () =>
        {
            await using var conn = (NpgsqlConnection) connection;
            await conn.OpenAsync();
        };

        await action.Should().ThrowAsync<Exception>();
    }
}