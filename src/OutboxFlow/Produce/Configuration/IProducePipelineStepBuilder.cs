using OutboxFlow.Configuration;

namespace OutboxFlow.Produce.Configuration;

/// <summary>
/// Represents a produce pipeline step builder.
/// </summary>
/// <typeparam name="TIn">Input message type.</typeparam>
/// <typeparam name="TOut">Output message type.</typeparam>
public interface IProducePipelineStepBuilder<TIn, TOut> : IPipelineStepBuilder<IProduceContext, TIn>
{
    /// <summary>
    /// Adds an asynchronous step to the pipeline.
    /// </summary>
    /// <param name="action">Step.</param>
    /// <typeparam name="TNext">Output parameter type.</typeparam>
    IProducePipelineStepBuilder<TOut, TNext> AddAsyncStep<TNext>(Func<TOut, IProduceContext, ValueTask<TNext>> action);

    /// <summary>
    /// Adds a synchronous step to the pipeline.
    /// </summary>
    /// <param name="action">Step.</param>
    /// <typeparam name="TNext">Output parameter type.</typeparam>
    IProducePipelineStepBuilder<TOut, TNext> AddSyncStep<TNext>(Func<TOut, IProduceContext, TNext> action);
}