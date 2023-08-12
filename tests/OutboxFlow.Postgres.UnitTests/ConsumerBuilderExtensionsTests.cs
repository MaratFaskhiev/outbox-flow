using Moq;
using OutboxFlow.Consume.Configuration;
using Xunit;

namespace OutboxFlow.Postgres.UnitTests;

public sealed class ConsumerBuilderExtensionsTests : IDisposable
{
    private readonly Mock<IConsumerBuilder> _builder = new(MockBehavior.Strict);

    public void Dispose()
    {
        Mock.VerifyAll(_builder);
    }

    [Fact]
    public void UsePostgres_SetsOutboxStorageRegistrar()
    {
        _builder.SetupSet(x => x.OutboxStorageRegistrar = It.IsAny<ConsumerOutboxStorageRegistrar>());

        _builder.Object.UsePostgres("connectionString");

        _builder.VerifySet(x => x.OutboxStorageRegistrar = It.IsAny<ConsumerOutboxStorageRegistrar>(), Times.Once);
    }
}