using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OutboxFlow.Abstractions;
using OutboxFlow.Configuration;
using OutboxFlow.Sample.Models;

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
                Log("\nCtrl+C sent. Shutdown..");
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

        services.AddOutbox(outboxBuilder =>
            outboxBuilder
                .AddProducer((sp, producer) =>
                {
                    var logger = sp.GetRequiredService<ILogger<IProducer>>();
                    producer
                        .ForMessage<SampleTextModel>(pipeline =>
                            pipeline
                                .AddStep<SampleMiddleware<SampleTextModel>, SampleTextModel>()
                                .AddStep(async (message, _) =>
                                {
                                    await Task.Delay(1);
                                    logger.LogInformation(message.Value);
                                    return message.Value.Length;
                                })
                                .AddStep(async (messageLength, _) =>
                                {
                                    await Task.Delay(1);
                                    logger.LogInformation(messageLength.ToString());
                                    return true;
                                })
                        )
                        .ForMessage<SampleNumericModel>(pipeline =>
                            pipeline
                                .AddStep<SampleMiddleware<SampleNumericModel>, SampleNumericModel>()
                                .AddStep(async (message, _) =>
                                {
                                    await Task.Delay(1);
                                    logger.LogInformation(message.Value.ToString());
                                    return message.Value.ToString();
                                })
                                .AddStep(async (messageText, _) =>
                                {
                                    await Task.Delay(1);
                                    logger.LogInformation(messageText);
                                    return true;
                                })
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