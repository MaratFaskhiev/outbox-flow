using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OutboxFlow.Abstractions;
using OutboxFlow.Sample.Models;

namespace OutboxFlow.Sample;

public sealed class Worker : BackgroundService
{
    private readonly IProducer _producer;
    private readonly ILogger<Worker> _logger;

    public Worker(IProducer producer, ILogger<Worker> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background worker is started.");

        var messageId = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            await _producer.ProduceAsync(
                new SampleTextModel(
                    $"Message #{messageId}"),
                new ProduceContext(null!, stoppingToken));

            messageId++;

            await Task.Delay(1000, stoppingToken);
        }
    }
}