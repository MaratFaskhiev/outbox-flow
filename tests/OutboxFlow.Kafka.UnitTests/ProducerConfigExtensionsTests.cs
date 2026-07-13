using Confluent.Kafka;
using FluentAssertions;
using Xunit;

namespace OutboxFlow.Kafka.UnitTests;

public sealed class ProducerConfigExtensionsTests
{
    [Fact]
    public void WithOutboxDefaults_SetsDefaults_WhenPropertiesAreNull()
    {
        var config = new ProducerConfig();

        config.WithOutboxDefaults();

        config.EnableIdempotence.Should().BeTrue();
        config.Acks.Should().Be(Acks.All);
    }

    [Fact]
    public void WithOutboxDefaults_DoesNotOverwrite_WhenEnableIdempotenceIsSet()
    {
        var config = new ProducerConfig
        {
            EnableIdempotence = false
        };

        config.WithOutboxDefaults();

        config.EnableIdempotence.Should().BeFalse();
        config.Acks.Should().Be(Acks.All);
    }

    [Fact]
    public void WithOutboxDefaults_DoesNotOverwrite_WhenAcksIsSet()
    {
        var config = new ProducerConfig
        {
            Acks = Acks.Leader
        };

        config.WithOutboxDefaults();

        config.EnableIdempotence.Should().BeTrue();
        config.Acks.Should().Be(Acks.Leader);
    }

    [Fact]
    public void WithOutboxDefaults_ReturnsSameInstance()
    {
        var config = new ProducerConfig();

        var result = config.WithOutboxDefaults();

        result.Should().BeSameAs(config);
    }
}