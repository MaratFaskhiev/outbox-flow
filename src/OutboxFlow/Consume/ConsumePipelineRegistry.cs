using OutboxFlow.Storage;

namespace OutboxFlow.Consume;

/// <inheritdoc />
public sealed class ConsumePipelineRegistry : IConsumePipelineRegistry
{
    private readonly IPipelineStep<IConsumeContext, IOutboxMessage>? _defaultPipeline;
    private readonly Dictionary<string, IPipelineStep<IConsumeContext, IOutboxMessage>> _pipelines;

    internal ConsumePipelineRegistry(
        Dictionary<string, IPipelineStep<IConsumeContext, IOutboxMessage>> pipelines,
        IPipelineStep<IConsumeContext, IOutboxMessage>? defaultPipeline)
    {
        _pipelines = pipelines;
        _defaultPipeline = defaultPipeline;
    }

    /// <inheritdoc />
    public IPipelineStep<IConsumeContext, IOutboxMessage> GetPipeline(string? destination = null)
    {
        if (destination == null)
        {
            if (_defaultPipeline == null)
                throw new InvalidOperationException("Default pipeline is not registered.");

            return _defaultPipeline;
        }

        if (!_pipelines.TryGetValue(destination, out var pipeline) && _defaultPipeline == null)
            throw new InvalidOperationException($"Pipeline for the destination \"{destination}\" is not registered.");

        return (pipeline ?? _defaultPipeline)!;
    }
}