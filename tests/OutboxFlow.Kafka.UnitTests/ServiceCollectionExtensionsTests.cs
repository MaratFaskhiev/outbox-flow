using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace OutboxFlow.Kafka.UnitTests;

public sealed class ServiceCollectionExtensionsTests : IDisposable
{
    private readonly Mock<IServiceCollection> _services = new(MockBehavior.Strict);

    public void Dispose()
    {
        Mock.VerifyAll(_services);
    }

    [Fact]
    public void AddKafka_RegistersKafkaProducerRegistry()
    {
        _services.Setup(x => x.Count).Returns(0);
        _services.Setup(x => x.Add(It.Is<ServiceDescriptor>(d =>
            d.ServiceType == typeof(IKafkaProducerRegistry) && d.ImplementationType == typeof(KafkaProducerRegistry))));
        _services.Setup(x => x.Add(It.Is<ServiceDescriptor>(d =>
            d.ServiceType == typeof(IKafkaProducerBuilder) && d.ImplementationType == typeof(KafkaProducerBuilder))));

        _services.Object.AddKafka();
    }
}