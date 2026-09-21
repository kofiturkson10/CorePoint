namespace CompanyPortal.Api.Models;

// What the API returns after a successful upload. The Id is what the client uses to download the file later.
public record DocumentInfo(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string UploadedBy,
    DateTime UploadedAt);
