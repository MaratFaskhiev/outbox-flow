using OutboxFlow.Configuration;

namespace OutboxFlow.Postgres;

/// <summary>
/// Extension methods for setting up PostgreSQL as an outbox storage.
/// </summary>
public static class ConsumerBuilderExtensions
{
    /// <summary>
    /// Configures the consumer to use an outbox storage based on PostgreSQL.
    /// </summary>
    /// <param name="connectionString">Database connection string.</param>
    /// <param name="builder">Outbox consumer builder.</param>
    public static IConsumerBuilder UsePostgres(
        this IConsumerBuilder builder,
        string connectionString)
    {
        builder.OutboxStorageRegistrar = new ConsumerOutboxStorageRegistrar(connectionString);

        return builder;
    }
}