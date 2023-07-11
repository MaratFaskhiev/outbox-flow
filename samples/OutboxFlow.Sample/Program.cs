using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OutboxFlow.Configuration;
using OutboxFlow.Kafka;
using OutboxFlow.Postgres;
using OutboxFlow.Produce.Configuration;
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

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };

        services
            // Register Apache Kafka dependencies
            .AddKafka()
            // Register the outbox dependencies
            .AddOutbox(outboxBuilder =>
                outboxBuilder
                    // Register the producer dependencies
                    .AddProducer(producer => producer
                        // Use PostgreSQL as an underlying storage
                        .UsePostgres()
                        // Configure pipeline for the SampleTextModel message type
                        .ForMessage<SampleTextModel>(pipeline =>
                            pipeline
                                // Add sample synchronous middleware
                                .AddSyncStep<LoggingMiddleware, SampleTextModel>()
                                // Convert message to the prototype model
                                .AddSyncStep((message, _) => new Protos.SampleTextModel
                                {
                                    Value = message.Value
                                })
                                // Serialize the prototype model to a byte array
                                .SerializeWithProtobuf()
                                // Set the message destination
                                .SetDestination("topic")
                                // Save the message to a storage
                                .Save()
                        )
                    )
                    // Register the consumer dependencies
                    .AddConsumer(consumer =>
                        consumer
                            // Use PostgreSQL as an underlying storage
                            .UsePostgres(context.Configuration.GetConnectionString("Postgres")!)
                            // Configure the default pipeline for outbox messages.
                            // Default route will be used for all destinations which are not configured explicitly
                            .SetDefaultRoute(pipeline => pipeline.SendToKafka(producerConfig))
                    )
            );

        services.AddScoped<LoggingMiddleware>();

        services.AddHostedService<Worker>();
    }
}