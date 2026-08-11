using System.Data;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OutboxFlow.Produce;
using OutboxFlow.Sample.Models;

namespace OutboxFlow.Sample;

#region docs_gs_worker

internal sealed class Worker : BackgroundService
{
    private static readonly Action<ILogger, Exception?> LogStarted =
        LoggerMessage.Define(LogLevel.Information, new EventId(0), "Background worker is started.");

    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(IServiceScopeFactory scopeFactory, ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    #region docs_qs_produce

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogStarted(_logger, null);

        var messageId = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            messageId++;

            using var scope = _scopeFactory.CreateScope();
            var producer = scope.ServiceProvider.GetRequiredService<IProducer>();
            var connection = scope.ServiceProvider.GetRequiredService<IDbConnection>();
            connection.Open();

            using var tx = connection.BeginTransaction();
            try
            {
                await producer.ProduceAsync(
                    new SampleTextModel($"Message #{messageId}"),
                    stoppingToken).ConfigureAwait(false);

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }

            await Task.Delay(10000, stoppingToken).ConfigureAwait(false);
        }
    }

    #endregion

    // ReSharper disable once UnusedMember.Glocal
    // ReSharper disable once UnusedMember.Local

    private async Task ProduceBatchExampleAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var producer = scope.ServiceProvider.GetRequiredService<IProducer>();
        var connection = scope.ServiceProvider.GetRequiredService<IDbConnection>();
        var dbConnection = (DbConnection) connection;
        await dbConnection.OpenAsync(stoppingToken).ConfigureAwait(false);

        using var tx = await dbConnection.BeginTransactionAsync(stoppingToken).ConfigureAwait(false);
        try
        {
            IReadOnlyCollection<SampleTextModel> messages = Enumerable.Range(0, 5).Select(i =>
                new SampleTextModel($"Batch message #{i}")).ToArray();

            await producer.ProduceAsync(
                messages, stoppingToken).ConfigureAwait(false);

            await tx.CommitAsync(stoppingToken).ConfigureAwait(false);
        }
        catch
        {
            await tx.RollbackAsync(stoppingToken).ConfigureAwait(false);
            throw;
        }
    }
}

#endregion
