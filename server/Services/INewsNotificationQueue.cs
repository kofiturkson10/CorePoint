using CompanyPortal.Api.Models;

namespace CompanyPortal.Api.Services;

// The JSON body of the message placed on the "news-notifications" queue. Picked up on the
// other end by the NewsNotificationFunction Azure Function (functions/CorePoint.Functions).
public record NewsNotificationMessage(int NewsId, string Title, DateTime PublishedAt);

public interface INewsNotificationQueue
{
    /// <summary>
    /// Sends a small notification message for a newly published news item. Never throws:
    /// notifying is a bonus, not a critical part of publishing news, so a missing queue
    /// configuration or a failed send must never stop the news item itself from being created.
    /// </summary>
    Task NotifyPublishedAsync(News news);
}
