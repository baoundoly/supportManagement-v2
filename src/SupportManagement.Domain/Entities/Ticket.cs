using SupportManagement.Domain.Common;
using SupportManagement.Domain.Enums;

namespace SupportManagement.Domain.Entities;

public class Ticket : BaseEntity
{
    public string TicketNo { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid? SubcategoryId { get; set; }
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.New;
    public Guid CreatedById { get; set; }
    public Guid? AssignedTeamId { get; set; }
    public Guid? AssignedUserId { get; set; }
    public string? AffectedModule { get; set; }
    public string? ReportedSource { get; set; }
    public DateTime? FirstResponseDueAt { get; set; }
    public DateTime? ResolutionDueAt { get; set; }
    public DateTime? FirstResponseAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int ReopenCount { get; set; } = 0;
    public bool IsSlaBreached { get; set; } = false;

    public TicketCategory Category { get; set; } = null!;
    public TicketSubcategory? Subcategory { get; set; }
    public User CreatedBy { get; set; } = null!;
    public User? AssignedUser { get; set; }
    public SupportTeam? AssignedTeam { get; set; }
    public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
    public ICollection<TicketAssignment> Assignments { get; set; } = new List<TicketAssignment>();
    public ICollection<TicketStatusHistory> StatusHistories { get; set; } = new List<TicketStatusHistory>();
    public ICollection<TicketResolution> Resolutions { get; set; } = new List<TicketResolution>();
    public ICollection<TicketEscalation> Escalations { get; set; } = new List<TicketEscalation>();
    public ICollection<TicketRating> Ratings { get; set; } = new List<TicketRating>();
    public ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();
    public ICollection<SlaTrackingLog> SlaTrackingLogs { get; set; } = new List<SlaTrackingLog>();
}
