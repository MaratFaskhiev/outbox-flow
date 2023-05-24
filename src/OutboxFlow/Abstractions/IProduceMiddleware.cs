namespace OutboxFlow.Abstractions;

/// <summary>
/// Represents a produce middleware.
/// </summary>
/// <typeparam name="TIn">Input message type.</typeparam>
/// <typeparam name="TOut">Output message type.</typeparam>
public interface IProduceMiddleware<TIn, TOut> : IMiddleware<IProduceContext, TIn, TOut>
{
}