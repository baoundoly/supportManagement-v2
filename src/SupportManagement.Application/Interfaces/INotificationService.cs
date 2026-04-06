using SupportManagement.Domain.Entities;

namespace SupportManagement.Application.Interfaces;

public interface INotificationService
{
    Task SendAsync(Notification notification, CancellationToken cancellationToken = default);
    Task SendToUserAsync(Guid userId, string title, string message, string? actionUrl = null, string? entityType = null, Guid? entityId = null, CancellationToken cancellationToken = default);
}
