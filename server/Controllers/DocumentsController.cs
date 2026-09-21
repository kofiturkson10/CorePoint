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

    // multipart/form-data with a single field named "file".
    // The size limit is set a bit above the file limit to leave room for the multipart overhead.
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

    [HttpGet("{id:guid}", Name = "DownloadDocument")]
    public async Task<IActionResult> DownloadAsync(Guid id)
    {
        var download = await _documentService.DownloadAsync(id);
        if (download is null)
        {
            return NotFound();
        }

        // Giving a file name makes the browser save the file (Content-Disposition: attachment)
        // instead of opening it, so an uploaded .html file is never run as a page on our domain.
        return File(download.Content, download.ContentType, download.FileName);
    }
}
