using CompanyPortal.Api.Data;
using CompanyPortal.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompanyPortal.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public EmployeesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<Employee>>> GetAllAsync(string? search, int page = 1, int pageSize = 10)
    {
        (page, pageSize) = PagingDefaults.Normalize(page, pageSize);

        // AsNoTracking: we only read, so EF doesn't need to track changes (faster).
        // Built up step by step and only sent to the database once, at the end.
        IQueryable<Employee> query = _dbContext.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(e =>
                e.Name.ToLower().Contains(normalizedSearch) ||
                e.Department.ToLower().Contains(normalizedSearch) ||
                e.Email.ToLower().Contains(normalizedSearch));
        }

        query = query.OrderBy(e => e.Name);

        var totalCount = await query.CountAsync();
        var employees = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(PagedResult<Employee>.Create(employees, totalCount, page, pageSize));
    }

    // The route Name lets CreateAsync build the Location header. ASP.NET Core strips the
    // "Async" suffix from action names, so nameof(GetByIdAsync) would not be found.
    [HttpGet("{id:int}", Name = "GetEmployeeById")]
    public async Task<ActionResult<Employee>> GetByIdAsync(int id)
    {
        var employee = await _dbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null)
        {
            return NotFound();
        }

        return Ok(employee);
    }

    [Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.HR))]
    [HttpPost]
    public async Task<ActionResult<Employee>> CreateAsync(EmployeeRequest request)
    {
        if (await IsEmailTakenAsync(request.Email, excludeEmployeeId: null))
        {
            return Conflict(new { message = "An employee with this email already exists." });
        }

        var employee = new Employee
        {
            Name = request.Name,
            Department = request.Department,
            Role = request.Role,
            Email = request.Email
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync(); // runs the INSERT and fills in employee.Id

        return CreatedAtRoute("GetEmployeeById", new { id = employee.Id }, employee);
    }

    [Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.HR))]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Employee>> UpdateAsync(int id, EmployeeRequest request)
    {
        // Tracked on purpose: EF notices which properties we change and updates only those
        var employee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (employee is null)
        {
            return NotFound();
        }

        if (await IsEmailTakenAsync(request.Email, excludeEmployeeId: id))
        {
            return Conflict(new { message = "An employee with this email already exists." });
        }

        employee.Name = request.Name;
        employee.Department = request.Department;
        employee.Role = request.Role;
        employee.Email = request.Email;

        await _dbContext.SaveChangesAsync(); // runs the UPDATE

        return Ok(employee);
    }

    [Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.HR))]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var employee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (employee is null)
        {
            return NotFound();
        }

        _dbContext.Employees.Remove(employee);
        await _dbContext.SaveChangesAsync(); // runs the DELETE

        return NoContent();
    }

    private async Task<bool> IsEmailTakenAsync(string email, int? excludeEmployeeId)
    {
        var normalizedEmail = email.ToLower();

        return await _dbContext.Employees.AnyAsync(e =>
            e.Email.ToLower() == normalizedEmail && e.Id != excludeEmployeeId);
    }
}
