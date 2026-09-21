namespace CompanyPortal.Api.Services;

// Bound to the "BlobStorage" section in appsettings.json.
// There is no secret here: authentication is done with Azure AD (managed identity), not with a key.
public class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    /// <summary>For example https://stfpxxxx.blob.core.windows.net (see the blobEndpoint output from the Bicep file).</summary>
    public string ServiceUri { get; set; } = string.Empty;

    public string ContainerName { get; set; } = "documents";
}
