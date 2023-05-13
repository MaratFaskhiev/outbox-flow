namespace OutboxFlow.Abstractions;

/// <summary>
/// Represents a synchronous produce middleware.
/// </summary>
/// <typeparam name="TIn">Input message type.</typeparam>
/// <typeparam name="TOut">Output message type.</typeparam>
public interface IProduceSyncMiddleware<TIn, TOut>
{
    /// <summary>
    /// Invokes a middleware.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="context">Produce context.</param>
    TOut Invoke(TIn message, IProduceContext context);
}