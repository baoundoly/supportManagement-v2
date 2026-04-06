using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class TicketAssignment : BaseEntity
{
    public Guid TicketId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? AssignedToTeamId { get; set; }
    public Guid AssignedByUserId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string? Remarks { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public User? AssignedToUser { get; set; }
    public SupportTeam? AssignedToTeam { get; set; }
    public User AssignedByUser { get; set; } = null!;
}
