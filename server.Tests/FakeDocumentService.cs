using System.Collections.Concurrent;
using CompanyPortal.Api.Models;
using CompanyPortal.Api.Services;

namespace CompanyPortal.Api.Tests;

// In-memory stand-in for BlobDocumentService, used only in tests - there's no real Azure
// Blob Storage to talk to here. Implements the same search/paging contract as
// BlobDocumentService, just backed by a dictionary instead of a blob container.
public class FakeDocumentService : IDocumentService
{
    private readonly ConcurrentDictionary<Guid, (DocumentInfo Info, byte[] Content)> _documents = new();

    public async Task<DocumentInfo> UploadAsync(Stream content, string fileName, string contentType, long sizeBytes, string uploadedBy)
    {
        var id = Guid.NewGuid();
        using var memoryStream = new MemoryStream();
        await content.CopyToAsync(memoryStream);

        var info = new DocumentInfo(id, fileName, contentType, sizeBytes, uploadedBy, DateTime.UtcNow);
        _documents[id] = (info, memoryStream.ToArray());

        return info;
    }

    public Task<PagedResult<DocumentInfo>> ListAsync(string? search, int page, int pageSize)
    {
        IEnumerable<DocumentInfo> documents = _documents.Values.Select(d => d.Info);

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

    public Task<DocumentDownload?> DownloadAsync(Guid id)
    {
        if (!_documents.TryGetValue(id, out var entry))
        {
            return Task.FromResult<DocumentDownload?>(null);
        }

        Stream stream = new MemoryStream(entry.Content);
        return Task.FromResult<DocumentDownload?>(new DocumentDownload(stream, entry.Info.ContentType, entry.Info.FileName));
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return Task.FromResult(_documents.TryRemove(id, out _));
    }
}
