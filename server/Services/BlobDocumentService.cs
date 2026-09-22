using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CompanyPortal.Api.Models;

namespace CompanyPortal.Api.Services;

public class BlobDocumentService : IDocumentService
{
    private const string FileNameMetadataKey = "originalFileName";
    private const string UploadedByMetadataKey = "uploadedBy";
    private const string UploadedAtMetadataKey = "uploadedAt";

    private readonly BlobContainerClient _containerClient;

    public BlobDocumentService(BlobContainerClient containerClient)
    {
        _containerClient = containerClient;
    }

    public async Task<DocumentInfo> UploadAsync(Stream content, string fileName, string contentType, long sizeBytes, string uploadedBy)
    {
        var id = Guid.NewGuid();
        var uploadedAt = DateTime.UtcNow;

        // The blob is named after a generated id, never after the user's file name.
        // That avoids name collisions and path tricks like "../".
        var blobClient = _containerClient.GetBlobClient(id.ToString("N"));

        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
            // Metadata values must be plain ASCII, so names with å, ä, ö are URL-encoded.
            Metadata = new Dictionary<string, string>
            {
                [FileNameMetadataKey] = Uri.EscapeDataString(fileName),
                [UploadedByMetadataKey] = Uri.EscapeDataString(uploadedBy),
                [UploadedAtMetadataKey] = uploadedAt.ToString("O")
            }
        };

        await blobClient.UploadAsync(content, options);

        return new DocumentInfo(id, fileName, contentType, sizeBytes, uploadedBy, uploadedAt);
    }

    public async Task<List<DocumentInfo>> ListAsync()
    {
        var documents = new List<DocumentInfo>();

        // One request lists the blobs together with their properties and metadata (name, size, type),
        // so no file content is transferred. Results come in pages; await foreach fetches them as needed.
        var listOptions = new GetBlobsOptions { Traits = BlobTraits.Metadata };
        await foreach (BlobItem blob in _containerClient.GetBlobsAsync(listOptions))
        {
            // Blobs are named after the document id. Anything else in the container isn't ours.
            if (!Guid.TryParse(blob.Name, out var id))
            {
                continue;
            }

            var metadata = blob.Metadata ?? new Dictionary<string, string>();

            var fileName = metadata.TryGetValue(FileNameMetadataKey, out var storedName)
                ? Uri.UnescapeDataString(storedName)
                : blob.Name;

            var uploadedBy = metadata.TryGetValue(UploadedByMetadataKey, out var storedUploader)
                ? Uri.UnescapeDataString(storedUploader)
                : "Unknown";

            // Falls back to the time Azure created the blob if our own timestamp is missing
            var uploadedAt = metadata.TryGetValue(UploadedAtMetadataKey, out var storedTime)
                && DateTime.TryParse(storedTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsedTime)
                ? parsedTime
                : blob.Properties.CreatedOn?.UtcDateTime ?? DateTime.MinValue;

            documents.Add(new DocumentInfo(
                id,
                fileName,
                blob.Properties.ContentType ?? "application/octet-stream",
                blob.Properties.ContentLength ?? 0,
                uploadedBy,
                uploadedAt));
        }

        // Azure lists blobs by name (random ids), so sort them here
        return documents.OrderByDescending(document => document.UploadedAt).ToList();
    }

    public async Task<DocumentDownload?> DownloadAsync(Guid id)
    {
        var blobClient = _containerClient.GetBlobClient(id.ToString("N"));

        try
        {
            // Streaming: the file is passed on to the client piece by piece instead of being loaded into memory.
            BlobDownloadStreamingResult result = await blobClient.DownloadStreamingAsync();

            var fileName = result.Details.Metadata.TryGetValue(FileNameMetadataKey, out var storedName)
                ? Uri.UnescapeDataString(storedName)
                : id.ToString("N");

            return new DocumentDownload(result.Content, result.Details.ContentType, fileName);
        }
        catch (RequestFailedException ex) when (ex.Status == StatusCodes.Status404NotFound)
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var blobClient = _containerClient.GetBlobClient(id.ToString("N"));
        var response = await blobClient.DeleteIfExistsAsync();
        return response.Value;
    }
}
