using Microsoft.Extensions.DependencyInjection;
using OutboxFlow.Abstractions;

namespace OutboxFlow.Configuration;

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
    /// <typeparam name="TIn">Current step input message type.</typeparam>
    /// <typeparam name="TOut">Current step output message type.</typeparam>
    public static ProducePipelineStepBuilder<TOut, TOut> AddStep<TMiddleware, TIn, TOut>(
        this ProducePipelineStepBuilder<TIn, TOut> step)
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
    /// <typeparam name="TIn">Middleware input message type.</typeparam>
    /// <typeparam name="TOut">Middleware output message type.</typeparam>
    /// <typeparam name="TStep">Current step input message type.</typeparam>
    public static ProducePipelineStepBuilder<TIn, TOut> AddStep<TMiddleware, TIn, TOut, TStep>(
        this ProducePipelineStepBuilder<TStep, TIn> step)
        where TMiddleware : IProduceMiddleware<TIn, TOut>
    {
        return step.AddStep(async (message, context) =>
        {
            var middleware = context.ServiceProvider.GetRequiredService<TMiddleware>();

            return await middleware.InvokeAsync(message, context).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Sets the message key.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <param name="keyProvider">Provides a key value.</param>
    /// <typeparam name="TIn">Step input message type.</typeparam>
    /// <typeparam name="TOut">Step output message type.</typeparam>
    public static ProducePipelineStepBuilder<TOut, TOut> SetKey<TIn, TOut>(
        this ProducePipelineStepBuilder<TIn, TOut> step, Func<TOut, byte[]> keyProvider)
    {
        return step.AddStep((message, context) =>
        {
            context.Key = keyProvider(message);
            return new ValueTask<TOut>(message);
        });
    }

    /// <summary>
    /// Sets the message destination.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <param name="destination">Destination.</param>
    /// <typeparam name="TIn">Step input message type.</typeparam>
    /// <typeparam name="TOut">Step output message type.</typeparam>
    public static ProducePipelineStepBuilder<TOut, TOut> SetDestination<TIn, TOut>(
        this ProducePipelineStepBuilder<TIn, TOut> step, string destination)
    {
        return step.AddStep((message, context) =>
        {
            context.Destination = destination;
            return new ValueTask<TOut>(message);
        });
    }

    /// <summary>
    /// Saves the message to the outbox storage.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TIn">Step input message type.</typeparam>
    /// <typeparam name="TOut">Step output message type.</typeparam>
    public static ProducePipelineStepBuilder<TOut, TOut> Save<TIn, TOut>(
        this ProducePipelineStepBuilder<TIn, TOut> step)
    {
        return step.AddStep(async (message, context) =>
        {
            var storage = context.ServiceProvider.GetRequiredService<IStorage>();
            await storage.SaveAsync(context).ConfigureAwait(false);

            return message;
        });
    }
}