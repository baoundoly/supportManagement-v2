using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string ActionType { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public Guid? PerformedById { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public User? PerformedBy { get; set; }
}
