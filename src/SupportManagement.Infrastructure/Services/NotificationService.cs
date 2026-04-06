using Microsoft.EntityFrameworkCore;
using SupportManagement.Application.Interfaces;
using SupportManagement.Domain.Entities;
using SupportManagement.Domain.Enums;
using SupportManagement.Infrastructure.Persistence;

namespace SupportManagement.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SendAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SendToUserAsync(Guid userId, string title, string message, string? actionUrl = null, string? entityType = null, Guid? entityId = null, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            EntityType = entityType,
            EntityId = entityId,
            Channel = NotificationChannel.InApp
        };
        await SendAsync(notification, cancellationToken);
    }
}
