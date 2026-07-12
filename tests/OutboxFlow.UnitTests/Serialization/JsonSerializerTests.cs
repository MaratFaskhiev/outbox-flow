using FluentAssertions;
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

        result.Should().NotBeNull();
        result!.Value.Should().Be(original.Value);
    }

    [Fact]
    public void Serialize_NullValue_ThrowsArgumentNullException()
    {
        FluentActions.Invoking(() => _serializer.Serialize<object?>(null)).Should().Throw<ArgumentNullException>();
    }

    private sealed class TestA
    {
        public string? Value { get; set; }
    }
}