using Microsoft.Extensions.DependencyInjection;
using OutboxFlow.Abstractions;

namespace OutboxFlow.Configuration;

/// <summary>
/// Extension methods for adding middlewares and steps to the produce pipeline.
/// </summary>
public static class ProducePipelineBuilderExtensions
{
    /// <summary>
    /// Adds a middleware to the pipeline.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <typeparam name="TMiddleware">Middleware type.</typeparam>
    /// <typeparam name="T">Message type.</typeparam>
    public static ProducePipelineStepBuilder<T, T> AddStep<TMiddleware, T>(
        this ProducePipelineBuilder<T> pipeline)
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
    public static ProducePipelineStepBuilder<TIn, TOut> AddStep<TMiddleware, TIn, TOut>(
        this ProducePipelineBuilder<TIn> pipeline)
        where TMiddleware : IProduceMiddleware<TIn, TOut>
    {
        return pipeline.AddStep(async (message, context) =>
        {
            var middleware = context.ServiceProvider.GetRequiredService<TMiddleware>();

            return await middleware.InvokeAsync(message, context).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Adds a synchronous middleware to the pipeline.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <typeparam name="TMiddleware">Middleware type.</typeparam>
    /// <typeparam name="T">Message type.</typeparam>
    public static ProducePipelineStepBuilder<T, T> AddSyncStep<TMiddleware, T>(
        this ProducePipelineBuilder<T> pipeline)
        where TMiddleware : IProduceSyncMiddleware<T, T>
    {
        return pipeline.AddSyncStep<TMiddleware, T, T>();
    }

    /// <summary>
    /// Adds a synchronous middleware to the pipeline.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <typeparam name="TMiddleware">Middleware type.</typeparam>
    /// <typeparam name="TIn">Middleware input message type.</typeparam>
    /// <typeparam name="TOut">Middleware output message type.</typeparam>
    public static ProducePipelineStepBuilder<TIn, TOut> AddSyncStep<TMiddleware, TIn, TOut>(
        this ProducePipelineBuilder<TIn> pipeline)
        where TMiddleware : IProduceSyncMiddleware<TIn, TOut>
    {
        return pipeline.AddSyncStep((message, context) =>
        {
            var middleware = context.ServiceProvider.GetRequiredService<TMiddleware>();

            return middleware.Invoke(message, context);
        });
    }

    /// <summary>
    /// Sets the message key.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <param name="keyProvider">Provides a key value.</param>
    /// <typeparam name="T">Pipeline input message type.</typeparam>
    public static ProducePipelineStepBuilder<T, T> SetKey<T>(
        this ProducePipelineBuilder<T> pipeline, Func<T, byte[]> keyProvider)
    {
        return pipeline.AddSyncStep((message, context) =>
        {
            context.Key = keyProvider(message);
            return message;
        });
    }

    /// <summary>
    /// Sets the message destination.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <param name="destination">Destination.</param>
    /// <typeparam name="T">Pipeline input message type.</typeparam>
    public static ProducePipelineStepBuilder<T, T> SetDestination<T>(
        this ProducePipelineBuilder<T> pipeline, string destination)
    {
        return pipeline.AddSyncStep((message, context) =>
        {
            context.Destination = destination;
            return message;
        });
    }

    /// <summary>
    /// Saves the message to the outbox storage.
    /// </summary>
    /// <param name="pipeline">Pipeline.</param>
    /// <typeparam name="T">Pipeline input message type.</typeparam>
    public static ProducePipelineStepBuilder<T, T> Save<T>(
        this ProducePipelineBuilder<T> pipeline)
    {
        return pipeline.AddStep(async (message, context) =>
        {
            var storage = context.ServiceProvider.GetRequiredService<IOutboxStorage>();
            await storage.SaveAsync(context).ConfigureAwait(false);

            return message;
        });
    }
}