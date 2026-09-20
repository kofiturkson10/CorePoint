using Microsoft.AspNetCore.Mvc;

namespace CompanyPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<object>> GetHealthAsync()
    {
        await Task.CompletedTask;
        return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
    }
}
