using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class TicketEscalation : BaseEntity
{
    public Guid TicketId { get; set; }
    public Guid EscalatedById { get; set; }
    public Guid? EscalatedToUserId { get; set; }
    public Guid? EscalatedToTeamId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime EscalatedAt { get; set; } = DateTime.UtcNow;

    public Ticket Ticket { get; set; } = null!;
    public User EscalatedBy { get; set; } = null!;
    public User? EscalatedToUser { get; set; }
    public SupportTeam? EscalatedToTeam { get; set; }
}
