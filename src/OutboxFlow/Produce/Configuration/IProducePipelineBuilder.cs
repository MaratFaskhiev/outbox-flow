namespace OutboxFlow.Produce.Configuration;

/// <summary>
/// Outbox produce pipeline builder.
/// </summary>
/// <typeparam name="T">Message type to produce.</typeparam>
public interface IProducePipelineBuilder<T> : IProducePipelineStepBuilder<T, T>
{
}