using System.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using OutboxFlow.Abstractions;
using OutboxFlow.Sample.Models;

namespace OutboxFlow.Sample;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IProducer _producer;

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
            messageId++;

            await _producer.ProduceAsync(
                new SampleTextModel(
                    $"Message #{messageId}"),
                Mock.Of<IDbTransaction>(),
                stoppingToken);

            await Task.Delay(10000, stoppingToken);
        }
    }
}