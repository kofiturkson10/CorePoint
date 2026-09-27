using System.Collections.Concurrent;
using CompanyPortal.Api.Models;
using CompanyPortal.Api.Services;

namespace CompanyPortal.Api.Tests;

// In-memory stand-in for BlobDocumentService, used only in tests - there's no real Azure
// Blob Storage to talk to here. Implements the same search/paging/download-url contract as
// BlobDocumentService, just backed by a dictionary instead of a blob container. File content
// is discarded rather than stored, since nothing in the contract reads it back anymore -
// downloads now return a URL, not the bytes themselves.
public class FakeDocumentService : IDocumentService
{
    private readonly ConcurrentDictionary<Guid, DocumentInfo> _documents = new();

    public Task<DocumentInfo> UploadAsync(Stream content, string fileName, string contentType, long sizeBytes, string uploadedBy)
    {
        var id = Guid.NewGuid();
        var info = new DocumentInfo(id, fileName, contentType, sizeBytes, uploadedBy, DateTime.UtcNow);
        _documents[id] = info;

        return Task.FromResult(info);
    }

    public Task<PagedResult<DocumentInfo>> ListAsync(string? search, int page, int pageSize)
    {
        IEnumerable<DocumentInfo> documents = _documents.Values;

        if (!string.IsNullOrWhiteSpace(search))
        {
            documents = documents.Where(d => d.FileName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var ordered = documents.OrderByDescending(d => d.UploadedAt).ToList();
        var totalCount = ordered.Count;

        var pageItems = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(PagedResult<DocumentInfo>.Create(pageItems, totalCount, page, pageSize));
    }

    public Task<string?> GetDownloadUrlAsync(Guid id)
    {
        if (!_documents.TryGetValue(id, out var entry))
        {
            return Task.FromResult<string?>(null);
        }

        // A stand-in for a real Azure user delegation SAS URL - there's no real Blob Storage
        // here, so this just needs to look like a URL for the controller/client contract to hold.
        var fakeUrl = $"https://fake-blob-storage.test/{id:N}?sas=fake&filename={Uri.EscapeDataString(entry.FileName)}";
        return Task.FromResult<string?>(fakeUrl);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return Task.FromResult(_documents.TryRemove(id, out _));
    }
}
