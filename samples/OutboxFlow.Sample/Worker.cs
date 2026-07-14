using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OutboxFlow.Produce;
using OutboxFlow.Sample.Models;

namespace OutboxFlow.Sample;

public sealed class Worker : BackgroundService
{
    private readonly string _connectionString;
    private readonly ILogger<Worker> _logger;
    private readonly IProducer _producer;

    public Worker(IProducer producer, IConfiguration configuration, ILogger<Worker> logger)
    {
        _producer = producer;
        _logger = logger;
        _connectionString = configuration.GetConnectionString("Postgres")!;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background worker is started.");

        var messageId = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            messageId++;

            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(stoppingToken);

                await using var transaction = await connection.BeginTransactionAsync(
                    IsolationLevel.ReadCommitted, stoppingToken);

                await _producer.ProduceAsync(
                    new SampleTextModel($"Message #{messageId}"),
                    transaction,
                    stoppingToken);

                await transaction.CommitAsync(stoppingToken);
            }

            await Task.Delay(10000, stoppingToken);
        }
    }

    // ReSharper disable once UnusedMember.Glocal
    // ReSharper disable once UnusedMember.Local
    private async Task ProduceBatchExampleAsync(CancellationToken stoppingToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(stoppingToken);

        await using var transaction = await connection.BeginTransactionAsync(
            IsolationLevel.ReadCommitted, stoppingToken);

        IReadOnlyCollection<SampleTextModel> messages = Enumerable.Range(0, 5).Select(i =>
            new SampleTextModel($"Batch message #{i}")).ToArray();

        await _producer.ProduceAsync(
            messages, transaction, stoppingToken);

        await transaction.CommitAsync(stoppingToken);
    }
}