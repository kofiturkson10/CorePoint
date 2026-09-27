using System.Collections.Concurrent;
using CompanyPortal.Api.Models;
using CompanyPortal.Api.Services;

namespace CompanyPortal.Api.Tests;

// In-memory stand-in for QueueNewsNotificationService, used only in tests - there's no real
// Azure Storage Queue to talk to here. Records every call instead of sending anything, so
// tests can verify NewsController invoked it with the right content.
public class FakeNewsNotificationQueue : INewsNotificationQueue
{
    private readonly ConcurrentQueue<NewsNotificationMessage> _sentMessages = new();

    public IReadOnlyCollection<NewsNotificationMessage> SentMessages => _sentMessages;

    public Task NotifyPublishedAsync(News news)
    {
        _sentMessages.Enqueue(new NewsNotificationMessage(news.Id, news.Title, news.PublishedAt));
        return Task.CompletedTask;
    }
}
