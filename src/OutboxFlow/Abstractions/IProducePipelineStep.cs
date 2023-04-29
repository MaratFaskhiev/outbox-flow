namespace OutboxFlow.Abstractions;

/// <summary>
/// Represents a produce pipeline step.
/// </summary>
/// <typeparam name="T">Input message type.</typeparam>
public interface IProducePipelineStep<T>
{
    /// <summary>
    /// Invokes a pipeline step.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="context">Produce context.</param>
    ValueTask InvokeAsync(T message, IProduceContext context);
}