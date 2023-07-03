using OutboxFlow.Storage;

namespace OutboxFlow.Consume;

/// <summary>
/// Consume pipeline registry.
/// </summary>
public interface IConsumePipelineRegistry
{
    /// <summary>
    /// Gets the pipeline by the destination.
    /// </summary>
    /// <param name="destination">Destination.</param>
    IPipelineStep<IConsumeContext, IOutboxMessage> GetPipeline(string? destination = null);
}