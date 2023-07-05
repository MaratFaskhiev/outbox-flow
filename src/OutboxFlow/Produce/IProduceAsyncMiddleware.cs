using OutboxFlow.Middleware;

namespace OutboxFlow.Produce;

/// <summary>
/// Represents an asynchronous produce middleware.
/// </summary>
/// <typeparam name="TIn">Input message type.</typeparam>
/// <typeparam name="TOut">Output message type.</typeparam>
public interface IProduceAsyncMiddleware<TIn, TOut> : IAsyncMiddleware<IProduceContext, TIn, TOut>
{
}