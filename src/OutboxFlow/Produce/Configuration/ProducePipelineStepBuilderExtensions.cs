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
        ArgumentNullException.ThrowIfNull(step);
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
        ArgumentNullException.ThrowIfNull(step);
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
        ArgumentNullException.ThrowIfNull(step);
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
        ArgumentNullException.ThrowIfNull(step);
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
        ArgumentNullException.ThrowIfNull(step);
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
        ArgumentNullException.ThrowIfNull(step);
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
        ArgumentNullException.ThrowIfNull(step);
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
        ArgumentNullException.ThrowIfNull(step);
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
        ArgumentNullException.ThrowIfNull(step);
        return step.AddAsyncStep(async (message, context) =>
        {
            var storage = context.ServiceProvider.GetRequiredService<IOutboxStorage>();
            await storage.SaveAsync(context).ConfigureAwait(false);

            return message;
        });
    }

    /// <summary>
    /// Iterates over each item in the collection, runs the configured sub-pipeline for each,
    /// and collects the resulting produce contexts.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <param name="configure">Configures the sub-pipeline for each item.</param>
    /// <typeparam name="TItem">Item type.</typeparam>
    public static IProducePipelineStepBuilder<
            IReadOnlyCollection<TItem>,
            IReadOnlyCollection<IProduceContext>>
        ForEach<TItem>(
            this IProducePipelineStepBuilder<
                IReadOnlyCollection<TItem>,
                IReadOnlyCollection<TItem>> step,
            Action<IProducePipelineBuilder<TItem>> configure)
    {
        ArgumentNullException.ThrowIfNull(step);
        ArgumentNullException.ThrowIfNull(configure);
        var subBuilder = new ProducePipelineBuilder<TItem>();
        configure(subBuilder);
        var subPipeline = subBuilder.Build();

        return step.AddAsyncStep(async (collection, context) =>
        {
            var contexts = new List<IProduceContext>();
            foreach (var item in collection)
            {
                var subContext = new ProduceContext(
                    context.ServiceProvider,
                    context.CancellationToken,
                    context.Headers);

                await subPipeline.RunAsync(item, subContext).ConfigureAwait(false);

                contexts.Add(subContext);
            }

            return (IReadOnlyCollection<IProduceContext>) contexts;
        });
    }

    /// <summary>
    /// Saves all collected produce contexts to the outbox storage in a single batch operation.
    /// </summary>
    /// <param name="step">Step.</param>
    public static IProducePipelineStepBuilder<
        IReadOnlyCollection<IProduceContext>,
        IReadOnlyCollection<IProduceContext>> SaveBatch<TSource>(
        this IProducePipelineStepBuilder<
            IReadOnlyCollection<TSource>,
            IReadOnlyCollection<IProduceContext>> step)
    {
        ArgumentNullException.ThrowIfNull(step);
        return step.AddAsyncStep(async (contexts, context) =>
        {
            if (contexts.Count == 0)
                return contexts;

            var storage = context.ServiceProvider.GetRequiredService<IOutboxStorage>();
            await storage.SaveBatchAsync(contexts).ConfigureAwait(false);

            return contexts;
        });
    }
}