using Microsoft.Extensions.Logging;
using OutboxFlow.Produce;
using OutboxFlow.Sample.Models;

namespace OutboxFlow.Sample;

#region docs_mw_async
internal sealed class AsyncLoggingMiddleware : IProduceAsyncMiddleware<SampleTextModel, SampleTextModel>
{
    private static readonly Action<ILogger, string, Exception?> LogMessage =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0),
            "Async produce middleware: {Value}");

    private readonly ILogger<AsyncLoggingMiddleware> _logger;

    public AsyncLoggingMiddleware(ILogger<AsyncLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async ValueTask<SampleTextModel> RunAsync(SampleTextModel message, IProduceContext context)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(context);

        await Task.Yield();

        LogMessage(_logger, message.Value, null);

        return message;
    }
}
#endregion