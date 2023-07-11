using Microsoft.Extensions.DependencyInjection;

namespace OutboxFlow.Consume.Configuration;

/// <summary>
/// Extension methods for adding middlewares and steps to the consume pipeline.
/// </summary>
public static class ConsumePipelineStepBuilderExtensions
{
    /// <summary>
    /// Adds an asynchronous middleware to the pipeline.
    /// </summary>
    /// <param name="step">Step.</param>
    /// <typeparam name="TMiddleware">Middleware type.</typeparam>
    /// <typeparam name="TIn">Current step input message type.</typeparam>
    /// <typeparam name="TOut">Current step output message type.</typeparam>
    public static IConsumePipelineStepBuilder<TOut, TOut> AddAsyncStep<TMiddleware, TIn, TOut>(
        this IConsumePipelineStepBuilder<TIn, TOut> step)
        where TMiddleware : IConsumeAsyncMiddleware<TOut, TOut>
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
    public static IConsumePipelineStepBuilder<TOut, TNext> AddAsyncStep<TMiddleware, TIn, TOut, TNext>(
        this IConsumePipelineStepBuilder<TIn, TOut> step)
        where TMiddleware : IConsumeAsyncMiddleware<TOut, TNext>
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
    /// <typeparam name="TIn">Current step input message type.</typeparam>
    /// <typeparam name="TOut">Current step output message type.</typeparam>
    public static IConsumePipelineStepBuilder<TOut, TOut> AddSyncStep<TMiddleware, TIn, TOut>(
        this IConsumePipelineStepBuilder<TIn, TOut> step)
        where TMiddleware : IConsumeSyncMiddleware<TOut, TOut>
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
    public static IConsumePipelineStepBuilder<TOut, TNext> AddSyncStep<TMiddleware, TIn, TOut, TNext>(
        this IConsumePipelineStepBuilder<TIn, TOut> step)
        where TMiddleware : IConsumeSyncMiddleware<TOut, TNext>
    {
        return step.AddSyncStep((message, context) =>
        {
            var middleware = context.ServiceProvider.GetRequiredService<TMiddleware>();

            return middleware.Run(message, context);
        });
    }
}