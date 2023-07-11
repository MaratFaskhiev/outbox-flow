using Microsoft.Extensions.DependencyInjection;
using OutboxFlow.Storage;

namespace OutboxFlow.Produce.Configuration;

/// <summary>
/// Extension methods for adding middlewares and steps to the produce pipeline.
/// </summary>
public static class ProducePipelineStepBuilderExtensions
{
    /// <summary>
    /// Adds an asynchronous middleware to the pipeline.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TMiddleware">Middleware type.</typeparam>
    /// <typeparam name="T">Message type.</typeparam>
    public static IProducePipelineStepBuilder<T, T> AddAsyncStep<TMiddleware, T>(
        this IProducePipelineStepBuilder<T, T> step)
        where TMiddleware : IProduceAsyncMiddleware<T, T>
    {
        return step.AddAsyncStep<TMiddleware, T, T, T>();
    }

    /// <summary>
    /// Adds an asynchronous middleware to the pipeline.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TMiddleware">Middleware type.</typeparam>
    /// <typeparam name="TIn">Current step input message type.</typeparam>
    /// <typeparam name="TOut">Current step output message type.</typeparam>
    public static IProducePipelineStepBuilder<TOut, TOut> AddAsyncStep<TMiddleware, TIn, TOut>(
        this IProducePipelineStepBuilder<TIn, TOut> step)
        where TMiddleware : IProduceAsyncMiddleware<TOut, TOut>
    {
        return step.AddAsyncStep<TMiddleware, TIn, TOut, TOut>();
    }

    /// <summary>
    /// Adds an asynchronous middleware to the pipeline.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TMiddleware">Middleware type.</typeparam>
    /// <typeparam name="TIn">Current step input message type.</typeparam>
    /// <typeparam name="TOut">Current step output message type.</typeparam>
    /// <typeparam name="TNext">Middleware output parameter type.</typeparam>
    public static IProducePipelineStepBuilder<TOut, TNext> AddAsyncStep<TMiddleware, TIn, TOut, TNext>(
        this IProducePipelineStepBuilder<TIn, TOut> step)
        where TMiddleware : IProduceAsyncMiddleware<TOut, TNext>
    {
        return step.AddAsyncStep(async (message, context) =>
        {
            var middleware = context.ServiceProvider.GetRequiredService<TMiddleware>();

            return await middleware.RunAsync(message, context).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Adds a synchronous middleware to the pipeline.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TMiddleware">Middleware type.</typeparam>
    /// <typeparam name="T">Message type.</typeparam>
    public static IProducePipelineStepBuilder<T, T> AddSyncStep<TMiddleware, T>(
        this IProducePipelineStepBuilder<T, T> step)
        where TMiddleware : IProduceSyncMiddleware<T, T>
    {
        return step.AddSyncStep<TMiddleware, T, T, T>();
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
        return step.AddSyncStep<TMiddleware, TIn, TOut, TOut>();
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

            return middleware.Run(message, context);
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
        return step.AddAsyncStep(async (message, context) =>
        {
            var storage = context.ServiceProvider.GetRequiredService<IOutboxStorage>();
            await storage.SaveAsync(context).ConfigureAwait(false);

            return message;
        });
    }
}