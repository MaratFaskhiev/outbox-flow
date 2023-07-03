namespace OutboxFlow.Configuration;

/// <summary>
/// Represents a pipeline step builder.
/// </summary>
/// <typeparam name="TContext">Context type.</typeparam>
/// <typeparam name="TIn">Input message type.</typeparam>
public interface IPipelineStepBuilder<TContext, TIn>
{
    /// <summary>
    /// Builds a produce pipeline step.
    /// </summary>
    IPipelineStep<TContext, TIn> Build();
}