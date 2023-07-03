using OutboxFlow.Storage;

namespace OutboxFlow.Consume.Configuration;

/// <summary>
/// Represents a consume pipeline builder.
/// </summary>
public interface IConsumePipelineBuilder : IConsumePipelineStepBuilder<IOutboxMessage, IOutboxMessage>
{
}