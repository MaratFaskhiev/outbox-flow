using OutboxFlow.Abstractions;

namespace OutboxFlow.Configuration;

/// <summary>
/// Represents a produce pipeline step builder.
/// </summary>
/// <typeparam name="T">Input message type.</typeparam>
public interface IProducePipelineStepBuilder<T>
{
    /// <summary>
    /// Builds a produce pipeline step.
    /// </summary>
    IProducePipelineStep<T> Build();
}