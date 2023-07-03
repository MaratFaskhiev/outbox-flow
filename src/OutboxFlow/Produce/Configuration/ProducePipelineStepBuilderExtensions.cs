using Microsoft.Extensions.DependencyInjection;
using OutboxFlow.Storage;

namespace OutboxFlow.Produce.Configuration;

/// <summary>
/// Extension methods for adding middlewares and steps to the produce pipeline.
/// </summary>
public static class ProducePipelineStepBuilderExtensions
{
    /// <summary>
    /// Adds a middleware to the pipeline.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TMiddleware">Middleware type.</typeparam>
    /// <typeparam name="T">Current step message type.</typeparam>
    public static IProducePipelineStepBuilder<T, T> AddStep<TMiddleware, T>(
        this IProducePipelineStepBuilder<T, T> step)
        where TMiddleware : IProduceMiddleware<T, T>
    {
        return step.AddStep(async (message, context) =>
        {
            var middleware = context.ServiceProvider.GetRequiredService<TMiddleware>();

            return await middleware.InvokeAsync(message, context).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Adds a middleware to the pipeline.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TMiddleware">Middleware type.</typeparam>
    /// <typeparam name="TIn">Current step input message type.</typeparam>
    /// <typeparam name="TOut">Current step output message type.</typeparam>
    public static IProducePipelineStepBuilder<TOut, TOut> AddStep<TMiddleware, TIn, TOut>(
        this IProducePipelineStepBuilder<TIn, TOut> step)
        where TMiddleware : IProduceMiddleware<TOut, TOut>
    {
        return step.AddStep(async (message, context) =>
        {
            var middleware = context.ServiceProvider.GetRequiredService<TMiddleware>();

            return await middleware.InvokeAsync(message, context).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Adds a middleware to the pipeline.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TMiddleware">Middleware type.</typeparam>
    /// <typeparam name="TIn">Current step input message type.</typeparam>
    /// <typeparam name="TOut">Current step output message type.</typeparam>
    /// <typeparam name="TNext">Middleware output parameter type.</typeparam>
    public static IProducePipelineStepBuilder<TOut, TNext> AddStep<TMiddleware, TIn, TOut, TNext>(
        this IProducePipelineStepBuilder<TIn, TOut> step)
        where TMiddleware : IProduceMiddleware<TOut, TNext>
    {
        return step.AddStep(async (message, context) =>
        {
            var middleware = context.ServiceProvider.GetRequiredService<TMiddleware>();

            return await middleware.InvokeAsync(message, context).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Adds a synchronous middleware to the pipeline.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TMiddleware">Middleware type.</typeparam>
    /// <typeparam name="T">Current step message type.</typeparam>
    public static IProducePipelineStepBuilder<T, T> AddSyncStep<TMiddleware, T>(
        this IProducePipelineStepBuilder<T, T> step)
        where TMiddleware : IProduceSyncMiddleware<T, T>
    {
        return step.AddSyncStep((message, context) =>
        {
            var middleware = context.ServiceProvider.GetRequiredService<TMiddleware>();

            return middleware.Invoke(message, context);
        });
    }

    /// <summary>
    /// Adds a synchronous middleware to the pipeline.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TMiddleware">Middleware type.</typeparam>
    /// <typeparam name="TIn">Current step input message type.</typeparam>
    /// <typeparam name="TOut">Current step output message type.</typeparam>
    public static IProducePipelineStepBuilder<TOut, TOut> AddSyncStep<TMiddleware, TIn, TOut>(
        this IProducePipelineStepBuilder<TIn, TOut> step)
        where TMiddleware : IProduceSyncMiddleware<TOut, TOut>
    {
        return step.AddSyncStep((message, context) =>
        {
            var middleware = context.ServiceProvider.GetRequiredService<TMiddleware>();

            return middleware.Invoke(message, context);
        });
    }

    /// <summary>
    /// Adds a synchronous middleware to the pipeline.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TMiddleware">Middleware type.</typeparam>
    /// <typeparam name="TIn">Current step input message type.</typeparam>
    /// <typeparam name="TOut">Current step output message type.</typeparam>
    /// <typeparam name="TNext">Middleware output parameter type.</typeparam>
    public static IProducePipelineStepBuilder<TOut, TNext> AddSyncStep<TMiddleware, TIn, TOut, TNext>(
        this IProducePipelineStepBuilder<TIn, TOut> step)
        where TMiddleware : IProduceSyncMiddleware<TOut, TNext>
    {
        return step.AddSyncStep((message, context) =>
        {
            var middleware = context.ServiceProvider.GetRequiredService<TMiddleware>();

            return middleware.Invoke(message, context);
        });
    }

    /// <summary>
    /// Sets the message key.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <param name="keyProvider">Provides a key value.</param>
    /// <typeparam name="TIn">Current step input message type.</typeparam>
    /// <typeparam name="TOut">Current step output message type.</typeparam>
    public static IProducePipelineStepBuilder<TOut, TOut> SetKey<TIn, TOut>(
        this IProducePipelineStepBuilder<TIn, TOut> step, Func<TOut, byte[]> keyProvider)
    {
        return step.AddSyncStep((message, context) =>
        {
            context.Key = keyProvider(message);
            return message;
        });
    }

    /// <summary>
    /// Sets the message destination.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <param name="destination">Destination.</param>
    /// <typeparam name="TIn">Current step input message type.</typeparam>
    /// <typeparam name="TOut">Current step output message type.</typeparam>
    public static IProducePipelineStepBuilder<TOut, TOut> SetDestination<TIn, TOut>(
        this IProducePipelineStepBuilder<TIn, TOut> step, string destination)
    {
        return step.AddSyncStep((message, context) =>
        {
            context.Destination = destination;
            return message;
        });
    }

    /// <summary>
    /// Saves the message to the outbox storage.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TIn">Current step input message type.</typeparam>
    /// <typeparam name="TOut">Current step output message type.</typeparam>
    public static IProducePipelineStepBuilder<TOut, TOut> Save<TIn, TOut>(
        this IProducePipelineStepBuilder<TIn, TOut> step)
    {
        return step.AddStep(async (message, context) =>
        {
            var storage = context.ServiceProvider.GetRequiredService<IOutboxStorage>();
            await storage.SaveAsync(context).ConfigureAwait(false);

            return message;
        });
    }
}