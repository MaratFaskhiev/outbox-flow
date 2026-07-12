using OutboxFlow.Storage;

namespace OutboxFlow.Consume;

/// <summary>
/// Consume pipeline registry.
/// </summary>
public interface IConsumePipelineRegistry
{
    /// <summary>
    /// Gets a consume pipeline for the specified destination.
    /// </summary>
    /// <param name="destination">Message destination.</param>
    /// <returns>Consume pipeline.</returns>
    IPipelineStep<IConsumeContext, IOutboxMessage> GetPipeline(string destination);

    /// <summary>
    /// Gets the default consume pipeline.
    /// </summary>
    /// <returns>Default consume pipeline.</returns>
    IPipelineStep<IConsumeContext, IOutboxMessage> GetPipeline();
}