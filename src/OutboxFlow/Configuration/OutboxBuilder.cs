using Microsoft.Extensions.DependencyInjection;
using OutboxFlow.Consume.Configuration;
using OutboxFlow.Produce.Configuration;

namespace OutboxFlow.Configuration;

/// <inheritdoc />
public sealed class OutboxBuilder : IOutboxBuilder
{
    private Action<ConsumerBuilder>? _consumerConfigureAction;
    private Action<ProducerBuilder>? _producerConfigureAction;

    /// <inheritdoc />
    public IOutboxBuilder AddProducer(Action<IProducerBuilder> configure)
    {
        if (_producerConfigureAction != null)
            throw new InvalidOperationException("An outbox producer is already configured.");
        _producerConfigureAction = configure;
        return this;
    }

    /// <inheritdoc />
    public IOutboxBuilder AddConsumer(Action<IConsumerBuilder> configure)
    {
        if (_consumerConfigureAction != null)
            throw new InvalidOperationException("An outbox consumer is already configured.");
        _consumerConfigureAction = configure;
        return this;
    }

    /// <inheritdoc />
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