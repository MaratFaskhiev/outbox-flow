namespace OutboxFlow.Abstractions;

/// <summary>
/// Represents a produce middleware.
/// </summary>
/// <typeparam name="TIn">Input message type.</typeparam>
/// <typeparam name="TOut">Output message type.</typeparam>
public interface IProduceMiddleware<TIn, TOut>
{
    /// <summary>
    /// Invokes a middleware.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="context">Produce context.</param>
    ValueTask<TOut> InvokeAsync(TIn message, IProduceContext context);
}