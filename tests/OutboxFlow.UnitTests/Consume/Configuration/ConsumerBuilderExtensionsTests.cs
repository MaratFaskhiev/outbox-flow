using System.Data;
using FluentAssertions;
using Moq;
using OutboxFlow.Consume.Configuration;
using OutboxFlow.Storage.Configuration;
using Xunit;

namespace OutboxFlow.UnitTests.Consume.Configuration;

public sealed class ConsumerBuilderExtensionsTests
{
    [Fact]
    public void SetOutboxStorageRegistrar_SetsOutboxStorageRegistrar()
    {
        var builder = new Mock<IConsumerBuilder>(MockBehavior.Strict);
        var outboxStorageRegistrar = Mock.Of<IOutboxStorageRegistrar>();

        builder.SetupSet(x => x.OutboxStorageRegistrar = outboxStorageRegistrar);

        builder.Object.SetOutboxStorageRegistrar(outboxStorageRegistrar);

        builder.VerifySet(x => x.OutboxStorageRegistrar = outboxStorageRegistrar);
    }

    [Fact]
    public void SetBatchSize_SetsBatchSize()
    {
        const int batchSize = 3;
        var builder = new Mock<IConsumerBuilder>(MockBehavior.Strict);

        builder.SetupSet(x => x.BatchSize = batchSize);

        var result = builder.Object.SetBatchSize(batchSize);

        result.Should().BeSameAs(builder.Object);
        builder.VerifySet(x => x.BatchSize = batchSize);
    }

    [Fact]
    public void SetConsumeDelay_SetsConsumeDelay()
    {
        var consumeDelay = TimeSpan.FromDays(1);
        var builder = new Mock<IConsumerBuilder>(MockBehavior.Strict);

        builder.SetupSet(x => x.ConsumeDelay = consumeDelay);

        var result = builder.Object.SetConsumeDelay(consumeDelay);

        result.Should().BeSameAs(builder.Object);
        builder.VerifySet(x => x.ConsumeDelay = consumeDelay);
    }

    [Fact]
    public void SetIsolationLevel_SetsIsolationLevel()
    {
        var isolationLevel = IsolationLevel.Serializable;
        var builder = new Mock<IConsumerBuilder>(MockBehavior.Strict);

        builder.SetupSet(x => x.IsolationLevel = isolationLevel);

        var result = builder.Object.SetIsolationLevel(isolationLevel);

        result.Should().BeSameAs(builder.Object);
        builder.VerifySet(x => x.IsolationLevel = isolationLevel);
    }

    [Fact]
    public void SetTimeout_SetsTimeout()
    {
        var timout = TimeSpan.FromDays(1);
        var builder = new Mock<IConsumerBuilder>(MockBehavior.Strict);

        builder.SetupSet(x => x.Timeout = timout);

        var result = builder.Object.SetTimeout(timout);

        result.Should().BeSameAs(builder.Object);
        builder.VerifySet(x => x.Timeout = timout);
    }
}