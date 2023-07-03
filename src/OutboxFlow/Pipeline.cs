namespace OutboxFlow;

/// <summary>
/// Outbox pipeline.
/// </summary>
/// <typeparam name="TContext">Context type.</typeparam>
/// <typeparam name="T">Message type.</typeparam>
public sealed class Pipeline<TContext, T> : IPipelineStep<TContext, T>
{
    private readonly IPipelineStep<TContext, T>? _step;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="step">First step.</param>
    public Pipeline(IPipelineStep<TContext, T>? step)
    {
        _step = step;
    }

    /// <inheritdoc />
    public async ValueTask InvokeAsync(T message, TContext context)
    {
        if (_step == null) return;

        await _step.InvokeAsync(message, context).ConfigureAwait(false);
    }
}