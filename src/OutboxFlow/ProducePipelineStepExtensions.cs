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
}