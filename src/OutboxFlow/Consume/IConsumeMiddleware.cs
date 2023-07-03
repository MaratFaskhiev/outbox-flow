using OutboxFlow.Middleware;

namespace OutboxFlow.Consume;

/// <summary>
/// Represents a consume middleware.
/// </summary>
/// <typeparam name="TIn">Input message type.</typeparam>
/// <typeparam name="TOut">Output message type.</typeparam>
public interface IConsumeMiddleware<TIn, TOut> : IMiddleware<IConsumeContext, TIn, TOut>
{
}