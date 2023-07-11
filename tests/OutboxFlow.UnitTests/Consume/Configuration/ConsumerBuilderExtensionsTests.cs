using System.Data;
using Moq;
using OutboxFlow.Consume.Configuration;
using OutboxFlow.Storage.Configuration;
using Xunit;

namespace OutboxFlow.UnitTests.Consume.Configuration;

public sealed class ConsumerBuilderExtensionsTests : IDisposable
{
    private readonly Mock<IConsumerBuilder> _builder;

    public ConsumerBuilderExtensionsTests()
    {
        _builder = new Mock<IConsumerBuilder>(MockBehavior.Strict);
    }

    public void Dispose()
    {
        Mock.VerifyAll(_builder);
    }

    [Fact]
    public void SetOutboxStorageRegistrar_SetsOutboxStorageRegistrar()
    {
        var outboxStorageRegistrar = Mock.Of<IOutboxStorageRegistrar>();

        _builder.SetupSet(x => x.OutboxStorageRegistrar = outboxStorageRegistrar);

        _builder.Object.SetOutboxStorageRegistrar(outboxStorageRegistrar);

        _builder.VerifySet(x => x.OutboxStorageRegistrar = outboxStorageRegistrar);
    }

    [Fact]
    public void SetBatchSize_SetsBatchSize()
    {
        var batchSize = 3;

        _builder.SetupSet(x => x.BatchSize = batchSize);

        _builder.Object.SetBatchSize(batchSize);

        _builder.VerifySet(x => x.BatchSize = batchSize);
    }

    [Fact]
    public void SetConsumeDelay_SetsConsumeDelay()
    {
        var consumeDelay = TimeSpan.FromDays(1);

        _builder.SetupSet(x => x.ConsumeDelay = consumeDelay);

        _builder.Object.SetConsumeDelay(consumeDelay);

        _builder.VerifySet(x => x.ConsumeDelay = consumeDelay);
    }

    [Fact]
    public void SetIsolationLevel_SetsIsolationLevel()
    {
        var isolationLevel = IsolationLevel.Serializable;

        _builder.SetupSet(x => x.IsolationLevel = isolationLevel);

        _builder.Object.SetIsolationLevel(isolationLevel);

        _builder.VerifySet(x => x.IsolationLevel = isolationLevel);
    }

    [Fact]
    public void SetTimeout_SetsTimeout()
    {
        var timout = TimeSpan.FromDays(1);

        _builder.SetupSet(x => x.Timeout = timout);

        _builder.Object.SetTimeout(timout);

        _builder.VerifySet(x => x.Timeout = timout);
    }
}