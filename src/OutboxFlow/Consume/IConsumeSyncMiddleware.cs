using OutboxFlow.Middleware;

namespace OutboxFlow.Consume;

/// <summary>
/// Represents a synchronous consume middleware.
/// </summary>
/// <typeparam name="TIn">Input message type.</typeparam>
/// <typeparam name="TOut">Output message type.</typeparam>
public interface IConsumeSyncMiddleware<TIn, TOut> : ISyncMiddleware<IConsumeContext, TIn, TOut>
{
}