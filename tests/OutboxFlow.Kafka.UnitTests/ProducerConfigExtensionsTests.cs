using Confluent.Kafka;
using FluentAssertions;
using Xunit;

namespace OutboxFlow.Kafka.UnitTests;

public sealed class ProducerConfigExtensionsTests
{
    [Fact]
    public void ApplyOutboxDefaults_SetsDefaults_WhenPropertiesAreNull()
    {
        var config = new ProducerConfig();

        config.ApplyOutboxDefaults();

        config.EnableIdempotence.Should().BeTrue();
        config.Acks.Should().Be(Acks.All);
    }

    [Fact]
    public void ApplyOutboxDefaults_DoesNotOverwrite_WhenEnableIdempotenceIsSet()
    {
        var config = new ProducerConfig
        {
            EnableIdempotence = false
        };

        config.ApplyOutboxDefaults();

        config.EnableIdempotence.Should().BeFalse();
        config.Acks.Should().Be(Acks.All);
    }

    [Fact]
    public void ApplyOutboxDefaults_DoesNotOverwrite_WhenAcksIsSet()
    {
        var config = new ProducerConfig
        {
            Acks = Acks.Leader
        };

        config.ApplyOutboxDefaults();

        config.EnableIdempotence.Should().BeTrue();
        config.Acks.Should().Be(Acks.Leader);
    }

    [Fact]
    public void ApplyOutboxDefaults_ReturnsSameInstance()
    {
        var config = new ProducerConfig();

        var result = config.ApplyOutboxDefaults();

        result.Should().BeSameAs(config);
    }

    [Fact]
    public void ApplyBatchDefaults_SetsDefaults_WhenPropertiesAreNull()
    {
        var config = new ProducerConfig();

        config.ApplyBatchDefaults();

        config.LingerMs.Should().Be(50);
        config.BatchSize.Should().Be(65536);
    }

    [Fact]
    public void ApplyBatchDefaults_DoesNotOverwrite_WhenLingerMsIsSet()
    {
        var config = new ProducerConfig
        {
            LingerMs = 100
        };

        config.ApplyBatchDefaults();

        config.LingerMs.Should().Be(100);
        config.BatchSize.Should().Be(65536);
    }

    [Fact]
    public void ApplyBatchDefaults_DoesNotOverwrite_WhenBatchSizeIsSet()
    {
        var config = new ProducerConfig
        {
            BatchSize = 131072
        };

        config.ApplyBatchDefaults();

        config.LingerMs.Should().Be(50);
        config.BatchSize.Should().Be(131072);
    }

    [Fact]
    public void ApplyBatchDefaults_ReturnsSameInstance()
    {
        var config = new ProducerConfig();

        var result = config.ApplyBatchDefaults();

        result.Should().BeSameAs(config);
    }
}