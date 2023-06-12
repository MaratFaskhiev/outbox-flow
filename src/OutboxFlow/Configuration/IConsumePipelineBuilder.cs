using OutboxFlow.Abstractions;

namespace OutboxFlow.Configuration;

/// <summary>
/// Represents a consume pipeline builder.
/// </summary>
public interface IConsumePipelineBuilder : IConsumePipelineStepBuilder<IOutboxMessage, IOutboxMessage>
{
}