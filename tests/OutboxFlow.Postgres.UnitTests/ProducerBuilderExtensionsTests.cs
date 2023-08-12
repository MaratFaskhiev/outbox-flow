using Moq;
using OutboxFlow.Produce.Configuration;
using Xunit;

namespace OutboxFlow.Postgres.UnitTests;

public sealed class ProducerBuilderExtensionsTests : IDisposable
{
    private readonly Mock<IProducerBuilder> _builder = new(MockBehavior.Strict);

    public void Dispose()
    {
        Mock.VerifyAll(_builder);
    }

    [Fact]
    public void UsePostgres_SetsOutboxStorageRegistrar()
    {
        _builder.SetupSet(x => x.OutboxStorageRegistrar = It.IsAny<ProducerOutboxStorageRegistrar>());

        _builder.Object.UsePostgres();

        _builder.VerifySet(x => x.OutboxStorageRegistrar = It.IsAny<ProducerOutboxStorageRegistrar>(), Times.Once);
    }
}