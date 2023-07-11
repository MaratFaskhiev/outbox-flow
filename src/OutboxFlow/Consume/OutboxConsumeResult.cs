namespace OutboxFlow.Consume;

/// <summary>
/// Contains result of consume operation.
/// </summary>
public sealed record OutboxConsumeResult
{
    /// <summary>
    /// Contains result of consume operation.
    /// </summary>
    /// <param name="IsSuccessful"><c>true</c> if messages were successfully consumed, otherwise <c>false</c>.</param>
    /// <param name="Count">The amount of consumed messages, if messages were successfully consumed.</param>
    public OutboxConsumeResult(bool IsSuccessful, int Count = default)
    {
        this.IsSuccessful = IsSuccessful;
        this.Count = Count;
    }

    /// <summary><c>true</c> if messages were successfully consumed, otherwise <c>false</c>.</summary>
    public bool IsSuccessful { get; }

    /// <summary>The amount of consumed messages, if messages were successfully consumed.</summary>
    public int Count { get; }
}