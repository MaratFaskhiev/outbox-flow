using Microsoft.Extensions.DependencyInjection;

namespace OutboxFlow.Configuration;

/// <summary>
/// Builds outbox pipelines.
/// </summary>
public sealed class OutboxBuilder
{
    private Action<ConsumerBuilder>? _consumerConfigureAction;
    private Action<ProducerBuilder>? _producerConfigureAction;

    /// <summary>
    /// Configures outbox produce pipelines.
    /// </summary>
    /// <param name="configure">Configure action.</param>
    public OutboxBuilder AddProducer(Action<ProducerBuilder> configure)
    {
        if (_producerConfigureAction != null)
            throw new InvalidOperationException("An outbox producer is already configured.");
        _producerConfigureAction = configure;
        return this;
    }

    /// <summary>
    /// Configures outbox consume pipelines.
    /// </summary>
    /// <param name="configure">Configure action.</param>
    public OutboxBuilder AddConsumer(Action<ConsumerBuilder> configure)
    {
        if (_consumerConfigureAction != null)
            throw new InvalidOperationException("An outbox consumer is already configured.");
        _consumerConfigureAction = configure;
        return this;
    }

    /// <summary>
    /// Builds outbox pipelines.
    /// </summary>
    /// <param name="services">Collection of service descriptors.</param>
    public void Build(IServiceCollection services)
    {
        BuildProducer(services);
        BuildConsumer(services);
    }

    private void BuildConsumer(IServiceCollection services)
    {
        if (_consumerConfigureAction == null) return;

        var builder = new ConsumerBuilder();
        _consumerConfigureAction(builder);
        builder.Build(services);
    }

    private void BuildProducer(IServiceCollection services)
    {
        if (_producerConfigureAction == null) return;

        var builder = new ProducerBuilder();
        _producerConfigureAction(builder);
        builder.Build(services);
    }
}