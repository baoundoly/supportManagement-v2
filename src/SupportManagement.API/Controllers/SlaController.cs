using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportManagement.Application.DTOs;
using SupportManagement.Domain.Entities;
using SupportManagement.Infrastructure.Persistence;

namespace SupportManagement.API.Controllers;

[Authorize]
public class SlaController : BaseController
{
    private readonly ApplicationDbContext _context;

    public SlaController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("policies")]
    public async Task<IActionResult> GetPolicies()
    {
        var policies = await _context.SlaPolicies
            .Include(s => s.Category)
            .Select(s => new SlaPolicyDto(
                s.Id, s.Name, s.Priority, s.CategoryId, s.Category != null ? s.Category.Name : null,
                s.ResponseTimeMinutes, s.ResolutionTimeMinutes, s.PauseDuringPendingUser, s.IsActive))
            .ToListAsync();
        return Ok(policies);
    }

    [HttpPost("policies")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePolicy([FromBody] CreateSlaPolicyRequest request)
    {
        var policy = new SlaPolicy
        {
            Name = request.Name,
            Priority = request.Priority,
            CategoryId = request.CategoryId,
            ResponseTimeMinutes = request.ResponseTimeMinutes,
            ResolutionTimeMinutes = request.ResolutionTimeMinutes,
            PauseDuringPendingUser = request.PauseDuringPendingUser
        };
        _context.SlaPolicies.Add(policy);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPolicies), null, policy.Id);
    }

    [HttpPut("policies/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePolicy(Guid id, [FromBody] UpdateSlaPolicyRequest request)
    {
        var policy = await _context.SlaPolicies.FindAsync(id);
        if (policy == null) return NotFound();

        policy.Name = request.Name;
        policy.ResponseTimeMinutes = request.ResponseTimeMinutes;
        policy.ResolutionTimeMinutes = request.ResolutionTimeMinutes;
        policy.PauseDuringPendingUser = request.PauseDuringPendingUser;
        policy.IsActive = request.IsActive;
        policy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Policy updated." });
    }

    [HttpGet("breaches")]
    public async Task<IActionResult> GetBreaches()
    {
        var breached = await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.AssignedUser)
            .Where(t => t.IsSlaBreached && t.Status != Domain.Enums.TicketStatus.Closed)
            .Select(t => new TicketSummaryDto(
                t.Id, t.TicketNo, t.Subject, t.Priority, t.Status,
                t.Category.Name, t.AssignedUser != null ? t.AssignedUser.FullName : null,
                null, t.IsSlaBreached, t.ResolutionDueAt, t.CreatedAt))
            .ToListAsync();
        return Ok(breached);
    }
}
