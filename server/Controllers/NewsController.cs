using System.Security.Claims;
using CompanyPortal.Api.Data;
using CompanyPortal.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompanyPortal.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public NewsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<News>>> GetAllAsync()
    {
        // Newest first
        var newsItems = await _dbContext.News
            .AsNoTracking()
            .OrderByDescending(n => n.PublishedAt)
            .ToListAsync();

        return Ok(newsItems);
    }

    [HttpGet("{id:int}", Name = "GetNewsById")]
    public async Task<ActionResult<News>> GetByIdAsync(int id)
    {
        var news = await _dbContext.News
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id);

        if (news is null)
        {
            return NotFound();
        }

        return Ok(news);
    }

    [HttpPost]
    public async Task<ActionResult<News>> CreateAsync(NewsRequest request)
    {
        var news = new News
        {
            Title = request.Title,
            Content = request.Content,
            // The author comes from the login cookie, not from the request, so it can't be faked
            Author = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown",
            PublishedAt = request.PublishedAt is null ? DateTime.UtcNow : ToUtc(request.PublishedAt.Value)
        };

        _dbContext.News.Add(news);
        await _dbContext.SaveChangesAsync();

        return CreatedAtRoute("GetNewsById", new { id = news.Id }, news);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<News>> UpdateAsync(int id, NewsRequest request)
    {
        var news = await _dbContext.News.FirstOrDefaultAsync(n => n.Id == id);
        if (news is null)
        {
            return NotFound();
        }

        news.Title = request.Title;
        news.Content = request.Content;
        // Author is never changed. PublishedAt is only changed when the client sends a new value.
        if (request.PublishedAt is not null)
        {
            news.PublishedAt = ToUtc(request.PublishedAt.Value);
        }

        await _dbContext.SaveChangesAsync();

        return Ok(news);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var news = await _dbContext.News.FirstOrDefaultAsync(n => n.Id == id);
        if (news is null)
        {
            return NotFound();
        }

        _dbContext.News.Remove(news);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    // A date without a time zone ("2026-09-21T10:00:00") is treated as UTC, not as server-local time
    private static DateTime ToUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Local
            ? value.ToUniversalTime()
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
