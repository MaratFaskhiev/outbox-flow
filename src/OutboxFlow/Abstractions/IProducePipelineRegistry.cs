namespace OutboxFlow.Abstractions;

/// <summary>
/// Produce pipeline registry.
/// </summary>
public interface IProducePipelineRegistry
{
    /// <summary>
    /// Gets the pipeline by the message type.
    /// </summary>
    /// <typeparam name="T">Message type.</typeparam>
    IProducePipelineStep<T> GetPipeline<T>();
}