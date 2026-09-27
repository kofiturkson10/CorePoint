using System.Security.Claims;
using CompanyPortal.Api.Models;
using CompanyPortal.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CompanyPortal.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<DocumentInfo>>> GetAllAsync(string? search, int page = 1, int pageSize = 10)
    {
        (page, pageSize) = PagingDefaults.Normalize(page, pageSize);

        var documents = await _documentService.ListAsync(search, page, pageSize);
        return Ok(documents);
    }

    // multipart/form-data with a single field named "file".
    // The size limit is set a bit above the file limit to leave room for the multipart overhead.
    // Documents aren't part of HR's Employees/News scope, so only Admin can write here.
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    [RequestSizeLimit(MaxFileSizeBytes + 1024 * 1024)]
    public async Task<ActionResult<DocumentInfo>> UploadAsync(IFormFile file)
    {
        if (file.Length == 0)
        {
            return BadRequest(new { message = "The file is empty." });
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest(new { message = "The file is too large. Maximum size is 10 MB." });
        }

        // Only the file name is used, not any path the browser may have included.
        var fileName = Path.GetFileName(file.FileName);
        var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
        // Who uploaded comes from the login cookie, not from the request, so it can't be faked.
        var uploadedBy = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

        await using var stream = file.OpenReadStream();
        var document = await _documentService.UploadAsync(stream, fileName, contentType, file.Length, uploadedBy);

        return CreatedAtRoute("DownloadDocument", new { id = document.Id }, document);
    }

    // Returns a short-lived download link instead of the file itself - the client fetches the
    // actual bytes directly from Blob Storage using this URL, not through the API. Still
    // requires login to obtain (class-level [Authorize] above), same as before; only the URL
    // itself, which expires in a few minutes, needs no further authentication once issued.
    [HttpGet("{id:guid}", Name = "DownloadDocument")]
    public async Task<ActionResult<object>> GetDownloadUrlAsync(Guid id)
    {
        var downloadUrl = await _documentService.GetDownloadUrlAsync(id);
        if (downloadUrl is null)
        {
            return NotFound();
        }

        return Ok(new { downloadUrl });
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var wasDeleted = await _documentService.DeleteAsync(id);
        if (!wasDeleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
