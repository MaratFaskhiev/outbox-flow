using FluentAssertions;
using Xunit;

namespace OutboxFlow.UnitTests;

public sealed class ClockTests
{
    private readonly Clock _clock = new();

    [Fact]
    public void UtcNow_ReturnsUtcDateTime()
    {
        _clock.UtcNow.Kind.Should().Be(DateTimeKind.Utc);
    }
}