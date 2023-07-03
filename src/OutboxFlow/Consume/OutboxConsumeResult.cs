namespace OutboxFlow.Consume;

/// <summary>
/// Contains result of consume operation.
/// </summary>
/// <param name="IsSuccessful"><c>true</c> if messages were successfully consumed, otherwise <c>false</c>.</param>
/// <param name="Count">The amount of consumed messages, if messages were successfully consumed.</param>
public sealed record OutboxConsumeResult(bool IsSuccessful, int Count = default);