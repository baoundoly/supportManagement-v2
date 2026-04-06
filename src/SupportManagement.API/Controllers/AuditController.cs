using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportManagement.Infrastructure.Persistence;

namespace SupportManagement.API.Controllers;

[Authorize(Roles = "Admin")]
public class AuditController : BaseController
{
    private readonly ApplicationDbContext _context;

    public AuditController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] string? entityName, [FromQuery] Guid? entityId, [FromQuery] string? actionType, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _context.AuditLogs.Include(a => a.PerformedBy).AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityName)) query = query.Where(a => a.EntityName == entityName);
        if (entityId.HasValue) query = query.Where(a => a.EntityId == entityId.Value);
        if (!string.IsNullOrWhiteSpace(actionType)) query = query.Where(a => a.ActionType == actionType);
        if (from.HasValue) query = query.Where(a => a.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(a => a.CreatedAt <= to.Value);

        var total = await query.CountAsync();
        var logs = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id, a.ActionType, a.EntityName, a.EntityId,
                PerformedBy = a.PerformedBy != null ? a.PerformedBy.FullName : null,
                a.OldValue, a.NewValue, a.IpAddress, a.CreatedAt
            })
            .ToListAsync();

        return Ok(new { items = logs, totalCount = total, page, pageSize });
    }
}
