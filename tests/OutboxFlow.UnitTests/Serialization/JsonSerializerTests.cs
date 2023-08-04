using OutboxFlow.Serialization;
using Xunit;

namespace OutboxFlow.UnitTests.Serialization;

public sealed class JsonSerializerTests
{
    private readonly JsonSerializer _serializer = new();

    [Fact]
    public void Serialize_ReturnsValidJson()
    {
        var original = new TestA
        {
            Value = "test"
        };

        var serializedResult = _serializer.Serialize(original);
        var result = System.Text.Json.JsonSerializer.Deserialize<TestA>(serializedResult);

        Assert.NotNull(result);
        Assert.Equal(original.Value, result.Value);
    }

    private sealed class TestA
    {
        public string? Value { get; set; }
    }
}