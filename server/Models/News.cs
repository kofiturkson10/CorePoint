namespace CompanyPortal.Api.Models;

public class News
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;

    /// <summary>Always stored and returned as UTC.</summary>
    public DateTime PublishedAt { get; set; }
}
