using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class SlaTrackingLog : BaseEntity
{
    public Guid TicketId { get; set; }
    public Guid SlaPolicyId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PausedAt { get; set; }
    public DateTime? ResumedAt { get; set; }
    public DateTime? BreachedAt { get; set; }
    public bool IsBreached { get; set; } = false;
    public string? BreachType { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public SlaPolicy SlaPolicy { get; set; } = null!;
}
