using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportManagement.Application.DTOs;
using SupportManagement.Domain.Entities;
using SupportManagement.Domain.Enums;
using SupportManagement.Infrastructure.Persistence;

namespace SupportManagement.API.Controllers;

[Authorize]
public class UsersController : BaseController
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,TeamLead")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] UserRole? role, [FromQuery] bool? isActive)
    {
        var query = _context.Users.Include(u => u.Department).Include(u => u.Team).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
        if (role.HasValue)
            query = query.Where(u => u.Role == role.Value);
        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        var users = await query.OrderBy(u => u.FullName).Select(u => MapDto(u)).ToListAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _context.Users.Include(u => u.Department).Include(u => u.Team).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();
        return Ok(MapDto(user));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest(new { message = "Email already exists." });

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Mobile = request.Mobile,
            EmployeeId = request.EmployeeId,
            Designation = request.Designation,
            Role = request.Role,
            DepartmentId = request.DepartmentId,
            TeamId = request.TeamId
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var created = await _context.Users.Include(u => u.Department).Include(u => u.Team).FirstAsync(u => u.Id == user.Id);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, MapDto(created));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.FullName = request.FullName;
        user.Mobile = request.Mobile;
        user.EmployeeId = request.EmployeeId;
        user.Designation = request.Designation;
        user.Role = request.Role;
        user.DepartmentId = request.DepartmentId;
        user.TeamId = request.TeamId;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "User updated." });
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] bool isActive)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { message = "User status updated." });
    }

    [HttpGet("roles")]
    public IActionResult GetRoles() => Ok(Enum.GetValues<UserRole>().Select(r => new { id = (int)r, name = r.ToString() }));

    private static UserDto MapDto(User u) => new(
        u.Id, u.FullName, u.Email, u.Mobile, u.EmployeeId,
        u.Designation, u.Role, u.DepartmentId, u.Department?.Name,
        u.TeamId, u.Team?.Name, u.IsActive, u.IsOnline,
        u.LastLoginAt, u.AvatarUrl, u.CreatedAt
    );
}
