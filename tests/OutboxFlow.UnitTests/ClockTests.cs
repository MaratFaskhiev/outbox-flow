using Xunit;

namespace OutboxFlow.UnitTests;

public sealed class ClockTests
{
    private readonly Clock _clock = new();

    [Fact]
    public void UtcNow_ReturnsUtcDateTime()
    {
        Assert.Equal(DateTimeKind.Utc, _clock.UtcNow.Kind);
    }
}