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
                Log("\nCtrl+C sent. Shutdown...");
                cts.Cancel();
            };

        using var host = Host.CreateDefaultBuilder(args)
            .UseDefaultServiceProvider((ctx, opt) =>
            {
                if (ctx.HostingEnvironment.IsDevelopment())
                {
                    opt.ValidateScopes = true;
                    opt.ValidateOnBuild = true;
                }
            })
            .ConfigureServices(ConfigureServices)
            .Build();

        await host.RunAsync(cts.Token);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(cfg => cfg.AddConsole());

        services
            .UsePostgres()
            .AddOutbox(outboxBuilder =>
                outboxBuilder
                    .AddProducer((sp, producer) =>
                    {
                        var logger = sp.GetRequiredService<ILogger<IProducer>>();
                        producer
                            .ForMessage<SampleTextModel>(pipeline =>
                                pipeline
                                    .AddSyncStep<SampleMiddleware<SampleTextModel>, SampleTextModel>()
                                    .AddStep(async (message, _) =>
                                    {
                                        // some async work
                                        await Task.Delay(1);

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
                            );
                    })
                    .AddConsumer((sp, consumerBuilder) => { }));

        services.AddHostedService<Worker>();

        services.AddScoped<SampleMiddleware<SampleTextModel>>();
        services.AddScoped<SampleMiddleware<SampleNumericModel>>();
    }

    private static void Log(string msg)
    {
        if (Environment.UserInteractive) Console.WriteLine(msg);
    }
}