namespace OutboxFlow.Produce;

internal static class ProduceContextExtensions
{
    public static void EnsureValid(this IProduceContext context)
    {
        if (string.IsNullOrEmpty(context.Destination))
            throw new InvalidOperationException("Destination must be defined.");

        if (context.Value == null) throw new InvalidOperationException("Value must be defined.");
    }
}