namespace OutboxFlow.Middleware;

/// <summary>
/// Represents a middleware.
/// </summary>
/// <typeparam name="TContext">Context type.</typeparam>
/// <typeparam name="TIn">Input message type.</typeparam>
/// <typeparam name="TOut">Output message type.</typeparam>
public interface IMiddleware<TContext, TIn, TOut>
{
    /// <summary>
    /// Invokes a middleware.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="context">Context.</param>
    ValueTask<TOut> InvokeAsync(TIn message, TContext context);
}