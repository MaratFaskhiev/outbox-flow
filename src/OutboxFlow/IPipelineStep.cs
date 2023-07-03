namespace OutboxFlow;

/// <summary>
/// Represents a pipeline step.
/// </summary>
/// <typeparam name="TContext">Context type.</typeparam>
/// <typeparam name="T">Input message type.</typeparam>
public interface IPipelineStep<TContext, T>
{
    /// <summary>
    /// Invokes a pipeline step.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="context">Context.</param>
    ValueTask InvokeAsync(T message, TContext context);
}