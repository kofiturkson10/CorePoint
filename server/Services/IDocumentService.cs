using CompanyPortal.Api.Models;

namespace CompanyPortal.Api.Services;

// The file content plus what the browser needs to save it under the right name.
// The caller must dispose the stream (returning it with File(...) in a controller does that).
public record DocumentDownload(Stream Content, string ContentType, string FileName);

public interface IDocumentService
{
    /// <summary>Stores the file in blob storage under a new id and returns information about it.</summary>
    Task<DocumentInfo> UploadAsync(Stream content, string fileName, string contentType, long sizeBytes, string uploadedBy);

    /// <summary>Returns information about all documents, newest first. The file contents are not downloaded.</summary>
    Task<List<DocumentInfo>> ListAsync();

    /// <summary>Returns the file, or null if no document with that id exists.</summary>
    Task<DocumentDownload?> DownloadAsync(Guid id);

    /// <summary>Deletes the document. Returns false if no document with that id exists.</summary>
    Task<bool> DeleteAsync(Guid id);
}
