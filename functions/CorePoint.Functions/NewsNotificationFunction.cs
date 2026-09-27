using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CorePoint.Functions;

public class NewsNotificationFunction
{
    private readonly ILogger<NewsNotificationFunction> _logger;

    public NewsNotificationFunction(ILogger<NewsNotificationFunction> logger)
    {
        _logger = logger;
    }

    // "AzureWebJobsStorage" is the default storage connection every Function App already has -
    // it's the same storage account the API writes to, so no separate connection is configured.
    // Binding straight to NewsNotification lets the isolated worker JSON-deserialize the queue
    // message body for us, instead of us doing it by hand from a raw string.
    [Function(nameof(NewsNotificationFunction))]
    public void Run([QueueTrigger("news-notifications", Connection = "AzureWebJobsStorage")] NewsNotification notification)
    {
        _logger.LogInformation(
            "Notifiering: ny nyhet publicerad - '{Title}' (Id: {NewsId})",
            notification.Title,
            notification.NewsId);
    }
}
