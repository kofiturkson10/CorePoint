namespace CompanyPortal.Api.Services;

// Bound to the "QueueStorage" section in appsettings.json.
// There is no secret here: authentication is done with Azure AD (managed identity), not with a key.
public class QueueStorageOptions
{
    public const string SectionName = "QueueStorage";

    /// <summary>For example https://stfpxxxx.queue.core.windows.net</summary>
    public string ServiceUri { get; set; } = string.Empty;

    public string QueueName { get; set; } = "news-notifications";
}
