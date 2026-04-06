using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportManagement.Application.DTOs;
using SupportManagement.Application.Interfaces;
using SupportManagement.Domain.Entities;
using SupportManagement.Domain.Enums;
using SupportManagement.Infrastructure.Persistence;

namespace SupportManagement.API.Controllers;

[Authorize]
public class TicketsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly INotificationService _notificationService;

    public TicketsController(ApplicationDbContext context, IFileStorageService fileStorage, INotificationService notificationService)
    {
        _context = context;
        _fileStorage = fileStorage;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTickets([FromQuery] TicketFilterRequest filter)
    {
        var query = _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.Subcategory)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedUser)
            .Include(t => t.AssignedTeam)
            .Include(t => t.Attachments)
            .AsQueryable();

        var role = CurrentUserRole;
        var userId = CurrentUserId;

        if (role == UserRole.EndUser.ToString())
            query = query.Where(t => t.CreatedById == userId);
        else if (role == UserRole.SupportAgent.ToString())
            query = query.Where(t => t.AssignedUserId == userId || t.CreatedById == userId);

        if (filter.Status.HasValue) query = query.Where(t => t.Status == filter.Status.Value);
        if (filter.Priority.HasValue) query = query.Where(t => t.Priority == filter.Priority.Value);
        if (filter.CategoryId.HasValue) query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
        if (filter.AssignedUserId.HasValue) query = query.Where(t => t.AssignedUserId == filter.AssignedUserId.Value);
        if (filter.AssignedTeamId.HasValue) query = query.Where(t => t.AssignedTeamId == filter.AssignedTeamId.Value);
        if (filter.IsSlaBreached.HasValue) query = query.Where(t => t.IsSlaBreached == filter.IsSlaBreached.Value);
        if (filter.FromDate.HasValue) query = query.Where(t => t.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue) query = query.Where(t => t.CreatedAt <= filter.ToDate.Value);
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            query = query.Where(t => t.Subject.Contains(filter.SearchTerm) || t.TicketNo.Contains(filter.SearchTerm) || t.Description.Contains(filter.SearchTerm));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(t => new TicketSummaryDto(
                t.Id, t.TicketNo, t.Subject, t.Priority, t.Status,
                t.Category.Name, t.AssignedUser != null ? t.AssignedUser.FullName : null,
                t.AssignedTeam != null ? t.AssignedTeam.Name : null,
                t.IsSlaBreached, t.ResolutionDueAt, t.CreatedAt))
            .ToListAsync();

        return Ok(new PagedResult<TicketSummaryDto>(items, total, filter.Page, filter.PageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicket(Guid id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.Subcategory)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedUser)
            .Include(t => t.AssignedTeam)
            .Include(t => t.Attachments).ThenInclude(a => a.UploadedBy)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null) return NotFound();
        return Ok(MapTicketDto(ticket));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
    {
        var ticketNo = await GenerateTicketNumberAsync();

        var ticket = new Ticket
        {
            TicketNo = ticketNo,
            Subject = request.Subject,
            Description = request.Description,
            CategoryId = request.CategoryId,
            SubcategoryId = request.SubcategoryId,
            Priority = request.Priority,
            AffectedModule = request.AffectedModule,
            ReportedSource = request.ReportedSource,
            CreatedById = CurrentUserId,
            Status = TicketStatus.New
        };

        // Set SLA deadlines
        var sla = await _context.SlaPolicies.FirstOrDefaultAsync(s => s.Priority == request.Priority && s.IsActive);
        if (sla != null)
        {
            ticket.FirstResponseDueAt = DateTime.UtcNow.AddMinutes(sla.ResponseTimeMinutes);
            ticket.ResolutionDueAt = DateTime.UtcNow.AddMinutes(sla.ResolutionTimeMinutes);
        }

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        // Add status history
        _context.TicketStatusHistories.Add(new TicketStatusHistory
        {
            TicketId = ticket.Id,
            OldStatus = TicketStatus.New,
            NewStatus = TicketStatus.New,
            ChangedById = CurrentUserId,
            ChangedAt = DateTime.UtcNow,
            Remarks = "Ticket created"
        });
        await _context.SaveChangesAsync();

        var created = await _context.Tickets
            .Include(t => t.Category).Include(t => t.Subcategory).Include(t => t.CreatedBy)
            .Include(t => t.AssignedUser).Include(t => t.AssignedTeam).Include(t => t.Attachments)
            .FirstAsync(t => t.Id == ticket.Id);

        return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, MapTicketDto(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTicket(Guid id, [FromBody] UpdateTicketRequest request)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        if (ticket.Status >= TicketStatus.Resolved)
            return BadRequest(new { message = "Cannot edit a resolved or closed ticket." });

        ticket.Subject = request.Subject;
        ticket.Description = request.Description;
        ticket.CategoryId = request.CategoryId;
        ticket.SubcategoryId = request.SubcategoryId;
        ticket.Priority = request.Priority;
        ticket.AffectedModule = request.AffectedModule;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Ticket updated." });
    }

    [HttpPost("{id}/assign")]
    [Authorize(Roles = "Admin,TeamLead")]
    public async Task<IActionResult> AssignTicket(Guid id, [FromBody] AssignTicketRequest request)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        var oldStatus = ticket.Status;
        ticket.AssignedUserId = request.AssignedToUserId;
        ticket.AssignedTeamId = request.AssignedToTeamId;
        ticket.Status = TicketStatus.Assigned;
        ticket.UpdatedAt = DateTime.UtcNow;

        _context.TicketAssignments.Add(new TicketAssignment
        {
            TicketId = id,
            AssignedToUserId = request.AssignedToUserId,
            AssignedToTeamId = request.AssignedToTeamId,
            AssignedByUserId = CurrentUserId,
            Remarks = request.Remarks
        });

        if (oldStatus != TicketStatus.Assigned)
        {
            _context.TicketStatusHistories.Add(new TicketStatusHistory
            {
                TicketId = id,
                OldStatus = oldStatus,
                NewStatus = TicketStatus.Assigned,
                ChangedById = CurrentUserId,
                Remarks = request.Remarks
            });
        }

        await _context.SaveChangesAsync();

        // Notify assigned user
        if (request.AssignedToUserId.HasValue)
            await _notificationService.SendToUserAsync(request.AssignedToUserId.Value, "Ticket Assigned", $"Ticket {ticket.TicketNo} has been assigned to you.", $"/tickets/{id}", "Ticket", id);

        return Ok(new { message = "Ticket assigned." });
    }

    [HttpPost("{id}/change-status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        if (ticket.Status == TicketStatus.Closed || ticket.Status == TicketStatus.Cancelled)
            return BadRequest(new { message = "Cannot change status of a closed or cancelled ticket." });

        var oldStatus = ticket.Status;
        ticket.Status = request.NewStatus;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (request.NewStatus == TicketStatus.InProgress && ticket.FirstResponseAt == null)
            ticket.FirstResponseAt = DateTime.UtcNow;

        _context.TicketStatusHistories.Add(new TicketStatusHistory
        {
            TicketId = id,
            OldStatus = oldStatus,
            NewStatus = request.NewStatus,
            ChangedById = CurrentUserId,
            Remarks = request.Remarks
        });

        await _context.SaveChangesAsync();

        await _notificationService.SendToUserAsync(ticket.CreatedById, "Ticket Status Updated",
            $"Ticket {ticket.TicketNo} status changed to {request.NewStatus}.", $"/tickets/{id}", "Ticket", id);

        return Ok(new { message = "Status updated." });
    }

    [HttpPost("{id}/resolve")]
    [Authorize(Roles = "Admin,TeamLead,SupportAgent")]
    public async Task<IActionResult> ResolveTicket(Guid id, [FromBody] ResolveTicketRequest request)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        var oldStatus = ticket.Status;
        ticket.Status = TicketStatus.Resolved;
        ticket.ResolvedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;

        _context.TicketResolutions.Add(new TicketResolution
        {
            TicketId = id,
            RootCause = request.RootCause,
            ResolutionSummary = request.ResolutionSummary,
            FixType = request.FixType,
            ClosureNotes = request.ClosureNotes,
            ResolvedById = CurrentUserId
        });

        _context.TicketStatusHistories.Add(new TicketStatusHistory
        {
            TicketId = id,
            OldStatus = oldStatus,
            NewStatus = TicketStatus.Resolved,
            ChangedById = CurrentUserId,
            Remarks = "Ticket resolved"
        });

        await _context.SaveChangesAsync();

        await _notificationService.SendToUserAsync(ticket.CreatedById, "Ticket Resolved",
            $"Ticket {ticket.TicketNo} has been resolved. Please confirm or reopen.", $"/tickets/{id}", "Ticket", id);

        return Ok(new { message = "Ticket resolved." });
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> CloseTicket(Guid id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        if (ticket.Status != TicketStatus.Resolved)
            return BadRequest(new { message = "Only resolved tickets can be closed." });

        var resolution = await _context.TicketResolutions.FirstOrDefaultAsync(r => r.TicketId == id);
        if (resolution != null)
        {
            resolution.UserConfirmed = true;
        }

        var oldStatus = ticket.Status;
        ticket.Status = TicketStatus.Closed;
        ticket.ClosedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;

        _context.TicketStatusHistories.Add(new TicketStatusHistory
        {
            TicketId = id,
            OldStatus = oldStatus,
            NewStatus = TicketStatus.Closed,
            ChangedById = CurrentUserId,
            Remarks = "User confirmed resolution"
        });

        await _context.SaveChangesAsync();
        return Ok(new { message = "Ticket closed." });
    }

    [HttpPost("{id}/reopen")]
    public async Task<IActionResult> ReopenTicket(Guid id, [FromBody] ReopenTicketRequest request)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        if (ticket.Status != TicketStatus.Resolved && ticket.Status != TicketStatus.Closed)
            return BadRequest(new { message = "Only resolved or closed tickets can be reopened." });

        var oldStatus = ticket.Status;
        ticket.Status = TicketStatus.Reopened;
        ticket.ReopenCount++;
        ticket.UpdatedAt = DateTime.UtcNow;

        _context.TicketStatusHistories.Add(new TicketStatusHistory
        {
            TicketId = id,
            OldStatus = oldStatus,
            NewStatus = TicketStatus.Reopened,
            ChangedById = CurrentUserId,
            Remarks = request.Reason
        });

        await _context.SaveChangesAsync();

        await _notificationService.SendToUserAsync(ticket.AssignedUserId ?? ticket.CreatedById,
            "Ticket Reopened", $"Ticket {ticket.TicketNo} has been reopened: {request.Reason}", $"/tickets/{id}", "Ticket", id);

        return Ok(new { message = "Ticket reopened." });
    }

    [HttpPost("{id}/attachments")]
    public async Task<IActionResult> UploadAttachment(Guid id, IFormFile file)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        if (file == null || file.Length == 0) return BadRequest(new { message = "No file provided." });

        await using var stream = file.OpenReadStream();
        var (storedFileName, filePath) = await _fileStorage.SaveFileAsync(stream, file.FileName, $"tickets/{id}");

        var attachment = new TicketAttachment
        {
            TicketId = id,
            UploadedById = CurrentUserId,
            FileName = file.FileName,
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            FilePath = filePath
        };
        _context.TicketAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        return Ok(new { id = attachment.Id, fileName = attachment.FileName, fileUrl = _fileStorage.GetFileUrl(filePath) });
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(Guid id)
    {
        var history = await _context.TicketStatusHistories
            .Include(h => h.ChangedBy)
            .Where(h => h.TicketId == id)
            .OrderBy(h => h.ChangedAt)
            .Select(h => new
            {
                h.Id, h.OldStatus, h.NewStatus,
                ChangedBy = h.ChangedBy.FullName,
                h.ChangedAt, h.Remarks
            })
            .ToListAsync();

        return Ok(history);
    }

    private async Task<string> GenerateTicketNumberAsync()
    {
        var count = await _context.Tickets.CountAsync();
        return $"TKT-{DateTime.UtcNow:yyyyMM}-{(count + 1):D5}";
    }

    private TicketDto MapTicketDto(Ticket ticket) => new(
        ticket.Id, ticket.TicketNo, ticket.Subject, ticket.Description,
        ticket.CategoryId, ticket.Category?.Name ?? string.Empty,
        ticket.SubcategoryId, ticket.Subcategory?.Name,
        ticket.Priority, ticket.Status,
        ticket.CreatedById, ticket.CreatedBy?.FullName ?? string.Empty,
        ticket.AssignedTeamId, ticket.AssignedTeam?.Name,
        ticket.AssignedUserId, ticket.AssignedUser?.FullName,
        ticket.AffectedModule, ticket.ReportedSource,
        ticket.FirstResponseDueAt, ticket.ResolutionDueAt,
        ticket.FirstResponseAt, ticket.ResolvedAt, ticket.ClosedAt,
        ticket.ReopenCount, ticket.IsSlaBreached,
        ticket.CreatedAt, ticket.UpdatedAt,
        ticket.Attachments.Select(a => new TicketAttachmentDto(
            a.Id, a.FileName, a.ContentType, a.FileSize,
            _fileStorage.GetFileUrl(a.FilePath), a.CreatedAt)).ToList()
    );
}
