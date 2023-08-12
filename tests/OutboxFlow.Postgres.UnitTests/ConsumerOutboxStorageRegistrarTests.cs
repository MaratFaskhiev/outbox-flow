using Microsoft.Extensions.DependencyInjection;
using Moq;
using OutboxFlow.Storage;
using Xunit;

namespace OutboxFlow.Postgres.UnitTests;

public sealed class ConsumerOutboxStorageRegistrarTests : IDisposable
{
    private readonly ConsumerOutboxStorageRegistrar _registrar = new("connectionString");
    private readonly Mock<IServiceCollection> _services = new(MockBehavior.Strict);

    public void Dispose()
    {
        Mock.VerifyAll(_services);
    }

    [Fact]
    public void Register_RegistersOutboxStorageDependencies()
    {
        _services.Setup(x => x.Count).Returns(0);
        _services.Setup(x => x.Add(It.Is<ServiceDescriptor>(d =>
            d.ServiceType == typeof(IOutboxStorage) && d.ImplementationType == typeof(OutboxStorage))));
        _services.Setup(x => x.Add(It.Is<ServiceDescriptor>(d =>
            d.ServiceType == typeof(IOutboxLockManager) && d.ImplementationType == typeof(OutboxLockManager))));
        _services.Setup(x => x.Add(It.Is<ServiceDescriptor>(d =>
            d.ServiceType == typeof(IDbConnectionFactory))));

        _registrar.Register(_services.Object);
    }
}