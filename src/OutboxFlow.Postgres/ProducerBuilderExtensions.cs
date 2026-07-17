using OutboxFlow.Produce.Configuration;

namespace OutboxFlow.Postgres;

/// <summary>
/// Extension methods for setting up PostgreSQL as an outbox storage.
/// </summary>
public static class ProducerBuilderExtensions
{
    /// <summary>
    /// Configures the producer to use an outbox storage based on PostgreSQL.
    /// </summary>
    /// <param name="builder">Outbox producer builder.</param>
    /// <param name="connectionString">Database connection string.</param>
    public static IProducerBuilder UsePostgres(this IProducerBuilder builder, string connectionString)
    {
        builder.OutboxStorageRegistrar = new ProducerOutboxStorageRegistrar(connectionString);

        return builder;
    }
}