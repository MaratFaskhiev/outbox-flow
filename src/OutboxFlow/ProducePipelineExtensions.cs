using Microsoft.Extensions.DependencyInjection;
using OutboxFlow.Abstractions;

namespace OutboxFlow;

/// <summary>
/// Extension methods for adding middlewares and steps to the produce pipeline.
/// </summary>
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

    /// <summary>
    /// Sets the message key.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <param name="keyProvider">Provides a key value.</param>
    /// <typeparam name="T">Pipeline input message type.</typeparam>
    public static ProducePipelineStep<T, T> SetKey<T>(
        this ProducePipeline<T> pipeline, Func<T, byte[]> keyProvider)
    {
        return pipeline.AddStep((message, context) =>
        {
            context.Key = keyProvider(message);
            return new ValueTask<T>(message);
        });
    }

    /// <summary>
    /// Sets the message destination.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <param name="destination">Destination.</param>
    /// <typeparam name="T">Pipeline input message type.</typeparam>
    public static ProducePipelineStep<T, T> SetDestination<T>(
        this ProducePipeline<T> pipeline, string destination)
    {
        return pipeline.AddStep((message, context) =>
        {
            context.Destination = destination;
            return new ValueTask<T>(message);
        });
    }

    /// <summary>
    /// Saves the message to the outbox storage.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <typeparam name="T">Pipeline input message type.</typeparam>
    public static ProducePipelineStep<T, T> Save<T>(
        this ProducePipeline<T> pipeline)
    {
        return pipeline.AddStep(async (message, context) =>
        {
            var storage = context.ServiceProvider.GetRequiredService<IStorage>();
            await storage.SaveAsync(context).ConfigureAwait(false);

            return message;
        });
    }
}