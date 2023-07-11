using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Moq;
using OutboxFlow.Produce;
using OutboxFlow.Produce.Configuration;
using OutboxFlow.Serialization;
using Xunit;

namespace OutboxFlow.UnitTests.Serialization;

public sealed class ProducePipelineStepBuilderExtensionsTests : IDisposable
{
    private readonly Mock<IProducePipelineStepBuilder<string, StringValue>> _protobufBuilder;
    private readonly Mock<IProducePipelineStepBuilder<int, string>> _builder;
    private readonly Mock<IProduceContext> _produceContext;
    private readonly Mock<ISerializer<byte[]>> _serializer;
    private readonly Mock<IServiceProvider> _serviceProvider;

    public ProducePipelineStepBuilderExtensionsTests()
    {
        _serializer = new Mock<ISerializer<byte[]>>(MockBehavior.Strict);
        _builder = new Mock<IProducePipelineStepBuilder<int, string>>(MockBehavior.Strict);
        _protobufBuilder = new Mock<IProducePipelineStepBuilder<string, StringValue>>(MockBehavior.Strict);
        _produceContext = new Mock<IProduceContext>(MockBehavior.Strict);
        _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
    }

    public void Dispose()
    {
        Mock.VerifyAll(_serializer, _builder, _produceContext, _serviceProvider, _protobufBuilder);
    }

    [Fact]
    public void Serialize_AddsSerializeValueStep()
    {
        var input = "test";
        var serializedInput = Guid.NewGuid().ToByteArray();

        Func<string, IProduceContext, string> pipelineStepAction = null!;
        _builder.Setup(x => x.AddSyncStep(It.IsAny<Func<string, IProduceContext, string>>()))
            .Returns((Func<string, IProduceContext, string> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IProducePipelineStepBuilder<string, string>>();
            });

        _produceContext.Setup(x => x.ServiceProvider).Returns(_serviceProvider.Object);

        _serviceProvider.Setup(x => x.GetService(typeof(ISerializer<byte[]>)))
            .Returns(_serializer.Object);

        _serializer.Setup(x => x.Serialize(input)).Returns(serializedInput);

        _produceContext.SetupSet(x => x.Value = serializedInput);

        _builder.Object.Serialize<ISerializer<byte[]>, int, string>();

        var result = pipelineStepAction!.Invoke(input, _produceContext.Object);

        Assert.Equal(input, result);
    }

    [Fact]
    public void SerializeKey_AddsSerializeKeyStep()
    {
        var input = "test";
        var key = Guid.NewGuid();
        var serializedKey = key.ToByteArray();

        Func<string, IProduceContext, string> pipelineStepAction = null!;
        _builder.Setup(x => x.AddSyncStep(It.IsAny<Func<string, IProduceContext, string>>()))
            .Returns((Func<string, IProduceContext, string> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IProducePipelineStepBuilder<string, string>>();
            });

        _serializer.Setup(x => x.Serialize(key)).Returns(serializedKey);

        _produceContext.SetupSet(x => x.Key = serializedKey);

        _produceContext.Setup(x => x.ServiceProvider).Returns(_serviceProvider.Object);

        _serviceProvider.Setup(x => x.GetService(typeof(ISerializer<byte[]>)))
            .Returns(_serializer.Object);

        _builder.Object.SerializeKey<ISerializer<byte[]>, int, string, Guid>(_ => key);

        var result = pipelineStepAction!.Invoke(input, _produceContext.Object);

        Assert.Equal(input, result);
    }

    [Fact]
    public void SerializeWithJson_AddsSerializeValueWithJsonStep()
    {
        var input = "test";
        var serializedInput = new JsonSerializer().Serialize(input);

        Func<string, IProduceContext, string> pipelineStepAction = null!;
        _builder.Setup(x => x.AddSyncStep(It.IsAny<Func<string, IProduceContext, string>>()))
            .Returns((Func<string, IProduceContext, string> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IProducePipelineStepBuilder<string, string>>();
            });

        _produceContext.SetupSet(x => x.Value = serializedInput);

        _builder.Object.SerializeWithJson();

        var result = pipelineStepAction!.Invoke(input, _produceContext.Object);

        Assert.Equal(input, result);
    }

    [Fact]
    public void SerializeKeyWithJson_AddsSerializeKeyWithJsonStep()
    {
        var input = "test";
        var key = Guid.NewGuid();
        var serializedKey = new JsonSerializer().Serialize(key);

        Func<string, IProduceContext, string> pipelineStepAction = null!;
        _builder.Setup(x => x.AddSyncStep(It.IsAny<Func<string, IProduceContext, string>>()))
            .Returns((Func<string, IProduceContext, string> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IProducePipelineStepBuilder<string, string>>();
            });

        _produceContext.SetupSet(x => x.Key = serializedKey);

        _builder.Object.SerializeKeyWithJson(_ => key);

        var result = pipelineStepAction!.Invoke(input, _produceContext.Object);

        Assert.Equal(input, result);
    }

    [Fact]
    public void SerializeWithProtobuf_AddsSerializeValueWithProtobufStep()
    {
        var input = new StringValue
        {
            Value = "test"
        };
        var serializedInput = input.ToByteArray();

        Func<StringValue, IProduceContext, StringValue> pipelineStepAction = null!;
        _protobufBuilder.Setup(x => x.AddSyncStep(It.IsAny<Func<StringValue, IProduceContext, StringValue>>()))
            .Returns((Func<StringValue, IProduceContext, StringValue> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IProducePipelineStepBuilder<StringValue, StringValue>>();
            });

        _produceContext.SetupSet(x => x.Value = serializedInput);

        _protobufBuilder.Object.SerializeWithProtobuf();

        var result = pipelineStepAction!.Invoke(input, _produceContext.Object);

        Assert.Equal(input, result);
    }

    [Fact]
    public void SerializeKeyWithProtobuf_AddsSerializeKeyWithProtobufStep()
    {
        var input = "test";
        var key = new StringValue
        {
            Value = "test"
        };
        var serializedKey = key.ToByteArray();

        Func<string, IProduceContext, string> pipelineStepAction = null!;
        _builder.Setup(x => x.AddSyncStep(It.IsAny<Func<string, IProduceContext, string>>()))
            .Returns((Func<string, IProduceContext, string> action) =>
            {
                pipelineStepAction = action;
                return Mock.Of<IProducePipelineStepBuilder<string, string>>();
            });

        _produceContext.SetupSet(x => x.Key = serializedKey);

        _builder.Object.SerializeKeyWithProtobuf(_ => key);

        var result = pipelineStepAction!.Invoke(input, _produceContext.Object);

        Assert.Equal(input, result);
    }
}