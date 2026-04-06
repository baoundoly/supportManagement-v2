using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportManagement.Application.DTOs;
using SupportManagement.Infrastructure.Persistence;

namespace SupportManagement.API.Controllers;

[Authorize]
public class NotificationsController : BaseController
{
    private readonly ApplicationDbContext _context;

    public NotificationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] bool? unreadOnly)
    {
        var query = _context.Notifications.Where(n => n.UserId == CurrentUserId);
        if (unreadOnly == true) query = query.Where(n => !n.IsRead);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationDto(
                n.Id, n.Title, n.Message, n.ActionUrl, n.EntityType, n.EntityId,
                n.IsRead, n.ReadAt, n.Channel, n.CreatedAt))
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == CurrentUserId);
        if (notification == null) return NotFound();

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var notifications = await _context.Notifications.Where(n => n.UserId == CurrentUserId && !n.IsRead).ToListAsync();
        foreach (var n in notifications)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
        return Ok(new { marked = notifications.Count });
    }
}
