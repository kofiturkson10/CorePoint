using System.ComponentModel.DataAnnotations;

namespace CompanyPortal.Api.Models;

// What the client sends when creating or updating a news item.
// Author is not included: the server sets it from the logged-in user.
public class NewsRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(10000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>Optional. When left out, a new item is published now and an edited item keeps its date.</summary>
    public DateTime? PublishedAt { get; set; }
}
