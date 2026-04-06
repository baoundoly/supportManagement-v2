using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportManagement.Application.DTOs;
using SupportManagement.Domain.Enums;
using SupportManagement.Infrastructure.Persistence;
using SupportManagement.Infrastructure.Services;

namespace SupportManagement.API.Controllers;

[Authorize(Roles = "Admin,TeamLead")]
public class ReportsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ReportService _reportService;

    public ReportsController(ApplicationDbContext context, ReportService reportService)
    {
        _context = context;
        _reportService = reportService;
    }

    [HttpGet("tickets-summary")]
    public async Task<IActionResult> GetTicketsSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow;

        var tickets = await _context.Tickets
            .Include(t => t.Category)
            .Where(t => t.CreatedAt >= fromDate && t.CreatedAt <= toDate)
            .ToListAsync();

        var agentPerf = await _context.Tickets
            .Include(t => t.AssignedUser)
            .Where(t => t.AssignedUserId != null && t.CreatedAt >= fromDate && t.CreatedAt <= toDate)
            .GroupBy(t => new { t.AssignedUserId, t.AssignedUser!.FullName })
            .Select(g => new AgentPerformanceDto(
                g.Key.FullName,
                g.Count(),
                g.Count(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed)))
            .ToListAsync();

        var resolved = tickets.Where(t => t.ResolvedAt.HasValue && t.FirstResponseAt.HasValue).ToList();
        var avgFirstResponse = resolved.Any() ? resolved.Average(t => (t.FirstResponseAt!.Value - t.CreatedAt).TotalMinutes) : 0;
        var avgResolution = resolved.Any() ? resolved.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalMinutes) : 0;

        var summary = new DashboardSummaryDto(
            tickets.Count,
            tickets.Count(t => t.Status is TicketStatus.Open or TicketStatus.Assigned or TicketStatus.InProgress),
            tickets.Count(t => t.Status == TicketStatus.Closed),
            tickets.Count(t => t.Status == TicketStatus.Resolved),
            tickets.Count(t => t.ResolutionDueAt < DateTime.UtcNow && t.Status < TicketStatus.Resolved),
            tickets.Count(t => t.IsSlaBreached),
            tickets.Count(t => t.ReopenCount > 0),
            Math.Round(avgFirstResponse, 2),
            Math.Round(avgResolution, 2),
            tickets.GroupBy(t => t.Category?.Name ?? "Unknown")
                   .Select(g => new CategoryTrendDto(g.Key, g.Count())).ToList(),
            agentPerf
        );

        return Ok(summary);
    }

    [HttpGet("tickets-summary/export/excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow;

        var tickets = await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.AssignedUser)
            .Include(t => t.AssignedTeam)
            .Where(t => t.CreatedAt >= fromDate && t.CreatedAt <= toDate)
            .Select(t => new TicketSummaryDto(
                t.Id, t.TicketNo, t.Subject, t.Priority, t.Status,
                t.Category.Name, t.AssignedUser != null ? t.AssignedUser.FullName : null,
                t.AssignedTeam != null ? t.AssignedTeam.Name : null,
                t.IsSlaBreached, t.ResolutionDueAt, t.CreatedAt))
            .ToListAsync();

        var bytes = _reportService.GenerateTicketExcel(tickets);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"tickets_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet("tickets-summary/export/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow;

        var tickets = await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.AssignedUser)
            .Include(t => t.AssignedTeam)
            .Where(t => t.CreatedAt >= fromDate && t.CreatedAt <= toDate)
            .Select(t => new TicketSummaryDto(
                t.Id, t.TicketNo, t.Subject, t.Priority, t.Status,
                t.Category.Name, t.AssignedUser != null ? t.AssignedUser.FullName : null,
                t.AssignedTeam != null ? t.AssignedTeam.Name : null,
                t.IsSlaBreached, t.ResolutionDueAt, t.CreatedAt))
            .ToListAsync();

        var bytes = _reportService.GenerateTicketPdf(tickets);
        return File(bytes, "application/pdf", $"tickets_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    [HttpGet("sla-compliance")]
    public async Task<IActionResult> GetSlaCompliance([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow;

        var total = await _context.Tickets.CountAsync(t => t.CreatedAt >= fromDate && t.CreatedAt <= toDate);
        var compliant = await _context.Tickets.CountAsync(t => t.CreatedAt >= fromDate && t.CreatedAt <= toDate && !t.IsSlaBreached);
        var rate = total > 0 ? Math.Round((double)compliant / total * 100, 2) : 0;

        return Ok(new SlaComplianceDto(total, compliant, rate));
    }

    [HttpGet("category-trend")]
    public async Task<IActionResult> GetCategoryTrend([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow;

        var trend = await _context.Tickets
            .Include(t => t.Category)
            .Where(t => t.CreatedAt >= fromDate && t.CreatedAt <= toDate)
            .GroupBy(t => t.Category.Name)
            .Select(g => new CategoryTrendDto(g.Key, g.Count()))
            .ToListAsync();

        return Ok(trend);
    }

    [HttpGet("agent-performance")]
    public async Task<IActionResult> GetAgentPerformance([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow;

        var perf = await _context.Tickets
            .Include(t => t.AssignedUser)
            .Where(t => t.AssignedUserId != null && t.CreatedAt >= fromDate && t.CreatedAt <= toDate)
            .GroupBy(t => new { t.AssignedUserId, t.AssignedUser!.FullName })
            .Select(g => new AgentPerformanceDto(
                g.Key.FullName,
                g.Count(),
                g.Count(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed)))
            .ToListAsync();

        return Ok(perf);
    }
}
