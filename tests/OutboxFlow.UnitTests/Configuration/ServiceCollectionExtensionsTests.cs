using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OutboxFlow.Configuration;
using Xunit;

namespace OutboxFlow.UnitTests.Configuration;

public sealed class ServiceCollectionExtensionsTests : IDisposable
{
    private readonly Mock<IServiceCollection> _services = new(MockBehavior.Strict);

    public void Dispose()
    {
        Mock.VerifyAll(_services);
    }

    [Fact]
    public void AddOutbox_ActionIsNull_ThrowsNullReferenceException()
    {
        FluentActions.Invoking(() => _services.Object.AddOutbox(null!)).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddOutbox_ActionIsNotNull_InvokesConfigurationAction()
    {
        _services.Setup(x => x.Count).Returns(0);
        _services.Setup(x => x.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(IClock))));

        var isInvoked = false;
        _services.Object.AddOutbox(_ => { isInvoked = true; });

        isInvoked.Should().BeTrue();
    }
}