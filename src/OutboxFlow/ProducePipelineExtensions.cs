using Microsoft.Extensions.DependencyInjection;
using OutboxFlow.Abstractions;

namespace OutboxFlow;

public static class ProducePipelineExtensions
{
    /// <summary>
    /// Adds a middleware to the pipeline.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <typeparam name="TMiddleware">Middleware type.</typeparam>
    /// <typeparam name="T">Message type.</typeparam>
    public static ProducePipelineStep<T, T> AddStep<TMiddleware, T>(
        this ProducePipeline<T> pipeline)
        where TMiddleware : IProduceMiddleware<T, T>
    {
        return pipeline.AddStep<TMiddleware, T, T>();
    }

    /// <summary>
    /// Adds a middleware to the pipeline.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <typeparam name="TMiddleware">Middleware type.</typeparam>
    /// <typeparam name="TIn">Middleware input message type.</typeparam>
    /// <typeparam name="TOut">Middleware output message type.</typeparam>
    public static ProducePipelineStep<TIn, TOut> AddStep<TMiddleware, TIn, TOut>(
        this ProducePipeline<TIn> pipeline)
        where TMiddleware : IProduceMiddleware<TIn, TOut>
    {
        return pipeline.AddStep(async (message, context) =>
        {
            var middleware = context.ServiceProvider.GetRequiredService<TMiddleware>();

            return await middleware.InvokeAsync(message, context).ConfigureAwait(false);
        });
    }
}