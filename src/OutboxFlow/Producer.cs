using System.Data;
using OutboxFlow.Abstractions;

namespace OutboxFlow;

/// <inheritdoc />
public sealed class Producer : IProducer
{
    private readonly IProducePipelineRegistry _registry;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="registry">Pipeline registry.</param>
    /// <param name="serviceProvider">Service provider.</param>
    public Producer(IProducePipelineRegistry registry, IServiceProvider serviceProvider)
    {
        _registry = registry;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async ValueTask ProduceAsync<T>(T message, IDbTransaction transaction, CancellationToken cancellationToken)
    {
        var pipeline = _registry.GetPipeline<T>();

        var context = new ProduceContext(transaction, _serviceProvider, cancellationToken);
        await pipeline.InvokeAsync(message, context).ConfigureAwait(false);
    }
}