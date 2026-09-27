using CompanyPortal.Api.Models;

namespace CompanyPortal.Api.Services;

public interface IDocumentService
{
    /// <summary>Stores the file in blob storage under a new id and returns information about it.</summary>
    Task<DocumentInfo> UploadAsync(Stream content, string fileName, string contentType, long sizeBytes, string uploadedBy);

    /// <summary>
    /// Returns a page of documents matching the search text (if any), newest first.
    /// The file contents are not downloaded.
    /// </summary>
    Task<PagedResult<DocumentInfo>> ListAsync(string? search, int page, int pageSize);

    /// <summary>
    /// Returns a short-lived, read-only download URL (a user delegation SAS) for the document,
    /// or null if no document with that id exists. The client downloads the file directly from
    /// Blob Storage using this URL - the file no longer passes through the API.
    /// </summary>
    Task<string?> GetDownloadUrlAsync(Guid id);

    /// <summary>Deletes the document. Returns false if no document with that id exists.</summary>
    Task<bool> DeleteAsync(Guid id);
}
