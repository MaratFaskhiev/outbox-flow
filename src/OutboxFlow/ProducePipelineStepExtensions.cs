using Microsoft.Extensions.DependencyInjection;
using OutboxFlow.Abstractions;

namespace OutboxFlow;

public static class ProducePipelineStepExtensions
{
    /// <summary>
    /// Adds a middleware to the pipeline.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TMiddleware">Middleware type.</typeparam>
    /// <typeparam name="TIn">Current step input message type.</typeparam>
    /// <typeparam name="TOut">Current step output message type.</typeparam>
    public static ProducePipelineStep<TOut, TOut> AddStep<TMiddleware, TIn, TOut>(
        this ProducePipelineStep<TIn, TOut> step)
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
    public static ProducePipelineStep<TIn, TOut> AddStep<TMiddleware, TIn, TOut, TStep>(
        this ProducePipelineStep<TStep, TIn> step)
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
    public static ProducePipelineStep<TOut, TOut> SetKey<TIn, TOut>(
        this ProducePipelineStep<TIn, TOut> step, Func<TOut, byte[]> keyProvider)
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
    public static ProducePipelineStep<TOut, TOut> SetDestination<TIn, TOut>(
        this ProducePipelineStep<TIn, TOut> step, string destination)
    {
        return step.AddStep((message, context) =>
        {
            context.Destination = destination;
            return new ValueTask<TOut>(message);
        });
    }
}