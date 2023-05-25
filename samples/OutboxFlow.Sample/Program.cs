using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OutboxFlow.Abstractions;
using OutboxFlow.Configuration;
using OutboxFlow.Postgres;
using OutboxFlow.Sample.Models;
using OutboxFlow.Serialization;

namespace OutboxFlow.Sample;

public static class Program
{
    public static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        if (Environment.UserInteractive)
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

        using var host = Host.CreateDefaultBuilder(args)
            .UseDefaultServiceProvider((ctx, opt) =>
            {
                if (!ctx.HostingEnvironment.IsDevelopment()) return;

                opt.ValidateScopes = true;
                opt.ValidateOnBuild = true;
            })
            .ConfigureServices(ConfigureServices)
            .Build();

        await host.RunAsync(cts.Token);
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddLogging(cfg => cfg.AddConsole());

        services
            .AddOutbox(outboxBuilder =>
                outboxBuilder
                    .AddProducer(producer => producer
                        .UsePostgres()
                        .ForMessage<SampleTextModel>(pipeline =>
                            pipeline
                                .AddSyncStep<SampleMiddleware<SampleTextModel>, SampleTextModel>()
                                .AddStep(async (message, ctx) =>
                                {
                                    var logger = ctx.ServiceProvider.GetRequiredService<ILogger<IProducer>>();

                                    // some async work
                                    await Task.Delay(1, ctx.CancellationToken);

                                    var protoModel = new Protos.SampleTextModel
                                    {
                                        Value = message.Value
                                    };

                                    logger.LogInformation("Message is converted.");

                                    return protoModel;
                                })
                                .SerializeToProtobuf()
                                .SetDestination("topic")
                                .Save()
                        )
                    )
                    .AddConsumer(consumer =>
                    {
                        consumer.IsolationLevel = IsolationLevel.Serializable;
                        consumer
                            .UsePostgres(context.Configuration.GetConnectionString("Postgres")!)
                            .Default(pipeline => pipeline
                                .AddStep(async (message, ctx) =>
                                {
                                    var logger = ctx.ServiceProvider
                                        .GetRequiredService<ILogger<IPipelineStep<IConsumeContext, IOutboxMessage>>>();

                                    // some async work
                                    await Task.Delay(1, ctx.CancellationToken);

                                    logger.LogInformation("Message is delivered.");

                                    return message;
                                }));
                    }));

        services.AddHostedService<Worker>();

        services.AddScoped<SampleMiddleware<SampleTextModel>>();
        services.AddScoped<SampleMiddleware<SampleNumericModel>>();
    }
}