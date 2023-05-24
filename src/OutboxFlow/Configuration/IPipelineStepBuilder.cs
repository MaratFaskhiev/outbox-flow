using OutboxFlow.Abstractions;

namespace OutboxFlow.Configuration;

/// <summary>
/// Represents a pipeline step builder.
/// </summary>
/// <typeparam name="TContext">Context type.</typeparam>
/// <typeparam name="T">Input message type.</typeparam>
public interface IPipelineStepBuilder<TContext, T>
{
    /// <summary>
    /// Builds a produce pipeline step.
    /// </summary>
    IPipelineStep<TContext, T> Build();
}