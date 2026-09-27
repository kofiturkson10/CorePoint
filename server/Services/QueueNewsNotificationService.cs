using System.Text.Json;
using Azure.Storage.Queues;
using CompanyPortal.Api.Models;

namespace CompanyPortal.Api.Services;

public class QueueNewsNotificationService : INewsNotificationQueue
{
    private readonly QueueClient? _queueClient;
    private readonly ILogger<QueueNewsNotificationService> _logger;

    // queueClient is null when QueueStorage:ServiceUri isn't configured (e.g. local
    // development) - see the registration in Program.cs.
    public QueueNewsNotificationService(QueueClient? queueClient, ILogger<QueueNewsNotificationService> logger)
    {
        _queueClient = queueClient;
        _logger = logger;
    }

    public async Task NotifyPublishedAsync(News news)
    {
        if (_queueClient is null)
        {
            return;
        }

        try
        {
            var message = new NewsNotificationMessage(news.Id, news.Title, news.PublishedAt);
            await _queueClient.SendMessageAsync(JsonSerializer.Serialize(message));
        }
        catch (Exception ex)
        {
            // The news item is already saved by the time this runs - a queue failure here
            // (e.g. Azure is down) must never fail the request that created it.
            _logger.LogError(ex, "Failed to send a news-notifications queue message for news {NewsId}.", news.Id);
        }
    }
}
