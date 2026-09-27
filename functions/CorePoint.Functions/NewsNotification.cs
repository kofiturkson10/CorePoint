namespace CorePoint.Functions;

// Matches the JSON shape the API places on the "news-notifications" queue
// (see NewsNotificationMessage in server/Services/INewsNotificationQueue.cs).
public record NewsNotification(int NewsId, string Title, DateTime PublishedAt);
