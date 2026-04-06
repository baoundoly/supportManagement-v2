using SupportManagement.Domain.Common;
using SupportManagement.Domain.Enums;

namespace SupportManagement.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;

    public User User { get; set; } = null!;
}
