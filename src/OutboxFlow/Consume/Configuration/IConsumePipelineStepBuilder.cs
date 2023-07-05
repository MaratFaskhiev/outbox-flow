using OutboxFlow.Configuration;

namespace OutboxFlow.Consume.Configuration;

/// <summary>
/// Represents a consume pipeline step builder.
/// </summary>
/// <typeparam name="TIn">Input message type.</typeparam>
/// <typeparam name="TOut">Output message type.</typeparam>
public interface IConsumePipelineStepBuilder<TIn, TOut> : IPipelineStepBuilder<IConsumeContext, TIn>
{
    /// <summary>
    /// Adds an asynchronous step to the pipeline.
    /// </summary>
    /// <param name="action">Step.</param>
    /// <typeparam name="TNext">Output parameter type.</typeparam>
    IConsumePipelineStepBuilder<TOut, TNext> AddAsyncStep<TNext>(Func<TOut, IConsumeContext, ValueTask<TNext>> action);

    /// <summary>
    /// Adds a synchronous step to the pipeline.
    /// </summary>
    /// <param name="action">Step.</param>
    /// <typeparam name="TNext">Output parameter type.</typeparam>
    IConsumePipelineStepBuilder<TOut, TNext> AddSyncStep<TNext>(Func<TOut, IConsumeContext, TNext> action);
}