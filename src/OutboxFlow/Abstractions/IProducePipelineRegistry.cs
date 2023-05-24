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
    IPipelineStep<IProduceContext, T> GetPipeline<T>();
}