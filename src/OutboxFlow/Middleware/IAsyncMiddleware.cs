namespace OutboxFlow.Middleware;

/// <summary>
/// Represents an asynchronous middleware.
/// </summary>
/// <typeparam name="TContext">Context type.</typeparam>
/// <typeparam name="TIn">Input message type.</typeparam>
/// <typeparam name="TOut">Output message type.</typeparam>
public interface IAsyncMiddleware<TContext, TIn, TOut>
{
    /// <summary>
    /// Runs a middleware.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="context">Context.</param>
    ValueTask<TOut> RunAsync(TIn message, TContext context);
}