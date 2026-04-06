using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportManagement.Application.DTOs;
using SupportManagement.Domain.Entities;
using SupportManagement.Infrastructure.Persistence;

namespace SupportManagement.API.Controllers;

[Authorize]
public class DepartmentsController : BaseController
{
    private readonly ApplicationDbContext _context;

    public DepartmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetDepartments()
    {
        var depts = await _context.Departments
            .Select(d => new DepartmentDto(d.Id, d.Name, d.Description, d.IsActive, d.CreatedAt))
            .ToListAsync();
        return Ok(depts);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
    {
        var dept = new Department { Name = request.Name, Description = request.Description };
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();
        return Ok(new { id = dept.Id, name = dept.Name });
    }

    [HttpGet("teams")]
    public async Task<IActionResult> GetTeams()
    {
        var teams = await _context.SupportTeams
            .Include(t => t.Department)
            .Include(t => t.TeamMembers)
            .Select(t => new SupportTeamDto(
                t.Id, t.Name, t.Description, t.DepartmentId,
                t.Department != null ? t.Department.Name : null,
                t.IsActive, t.TeamMembers.Count))
            .ToListAsync();
        return Ok(teams);
    }

    [HttpPost("teams")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTeam([FromBody] CreateSupportTeamRequest request)
    {
        var team = new SupportTeam { Name = request.Name, Description = request.Description, DepartmentId = request.DepartmentId };
        _context.SupportTeams.Add(team);
        await _context.SaveChangesAsync();
        return Ok(new { id = team.Id });
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetTicketCategories()
    {
        var cats = await _context.TicketCategories
            .Include(c => c.Subcategories)
            .Where(c => c.IsActive)
            .Select(c => new
            {
                c.Id, c.Name, c.Description,
                Subcategories = c.Subcategories.Where(s => s.IsActive).Select(s => new { s.Id, s.Name })
            })
            .ToListAsync();
        return Ok(cats);
    }

    [HttpPost("categories")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateDepartmentRequest request)
    {
        var cat = new TicketCategory { Name = request.Name, Description = request.Description };
        _context.TicketCategories.Add(cat);
        await _context.SaveChangesAsync();
        return Ok(new { id = cat.Id });
    }

    [HttpPost("categories/{categoryId}/subcategories")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateSubcategory(Guid categoryId, [FromBody] CreateDepartmentRequest request)
    {
        var sub = new TicketSubcategory { CategoryId = categoryId, Name = request.Name, Description = request.Description };
        _context.TicketSubcategories.Add(sub);
        await _context.SaveChangesAsync();
        return Ok(new { id = sub.Id });
    }
}
