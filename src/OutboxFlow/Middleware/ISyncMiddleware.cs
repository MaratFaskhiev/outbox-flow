namespace OutboxFlow.Middleware;

/// <summary>
/// Represents a synchronous middleware.
/// </summary>
/// <typeparam name="TContext">Context type.</typeparam>
/// <typeparam name="TIn">Input message type.</typeparam>
/// <typeparam name="TOut">Output message type.</typeparam>
public interface ISyncMiddleware<TContext, TIn, TOut>
{
    /// <summary>
    /// Runs a middleware.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="context">Context.</param>
    TOut Run(TIn message, TContext context);
}