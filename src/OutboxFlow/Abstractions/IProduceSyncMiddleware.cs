namespace OutboxFlow.Abstractions;

/// <summary>
/// Represents a synchronous produce middleware.
/// </summary>
/// <typeparam name="TIn">Input message type.</typeparam>
/// <typeparam name="TOut">Output message type.</typeparam>
public interface IProduceSyncMiddleware<TIn, TOut> : ISyncMiddleware<IProduceContext, TIn, TOut>
{
}