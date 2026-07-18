using System.Transactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OutboxFlow.Produce;
using OutboxFlow.Sample.Models;

namespace OutboxFlow.Sample;

internal sealed class Worker : BackgroundService
{
    private static readonly Action<ILogger, Exception?> LogStarted =
        LoggerMessage.Define(LogLevel.Information, new EventId(0), "Background worker is started.");

    private readonly ILogger<Worker> _logger;
    private readonly IProducer _producer;

    public Worker(IProducer producer, IConfiguration configuration, ILogger<Worker> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogStarted(_logger, null);

        var messageId = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            messageId++;

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _producer.ProduceAsync(
                    new SampleTextModel($"Message #{messageId}"),
                    stoppingToken).ConfigureAwait(false);

                scope.Complete();
            }

            await Task.Delay(10000, stoppingToken).ConfigureAwait(false);
        }
    }

    // ReSharper disable once UnusedMember.Glocal
    // ReSharper disable once UnusedMember.Local
    private async Task ProduceBatchExampleAsync(CancellationToken stoppingToken)
    {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            IReadOnlyCollection<SampleTextModel> messages = Enumerable.Range(0, 5).Select(i =>
                new SampleTextModel($"Batch message #{i}")).ToArray();

            await _producer.ProduceAsync(
                messages, stoppingToken).ConfigureAwait(false);

            scope.Complete();
        }
    }
}