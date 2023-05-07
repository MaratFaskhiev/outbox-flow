using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OutboxFlow.Abstractions;
using OutboxFlow.Sample.Models;

namespace OutboxFlow.Sample;

public sealed class Worker : BackgroundService
{
    private readonly IProducer _producer;
    private readonly string? _connectionString;
    private readonly ILogger<Worker> _logger;

    public Worker(IProducer producer, IConfiguration configuration, ILogger<Worker> logger)
    {
        _producer = producer;
        _logger = logger;
        _connectionString = configuration.GetConnectionString("Postgres");
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

                await using var transaction = await connection.BeginTransactionAsync(stoppingToken);

                await _producer.ProduceAsync(
                    new SampleTextModel($"Message #{messageId}"),
                    transaction,
                    stoppingToken);

                await transaction.CommitAsync(stoppingToken);
            }

            await Task.Delay(10000, stoppingToken);
        }
    }
}