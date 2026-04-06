using SupportManagement.Domain.Common;
using SupportManagement.Domain.Enums;

namespace SupportManagement.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string? EmployeeId { get; set; }
    public string? Designation { get; set; }
    public UserRole Role { get; set; } = UserRole.EndUser;
    public Guid? DepartmentId { get; set; }
    public Guid? TeamId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsOnline { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }
    public string? AvatarUrl { get; set; }

    public Department? Department { get; set; }
    public SupportTeam? Team { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();
    public ICollection<TicketAssignment> Assignments { get; set; } = new List<TicketAssignment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
