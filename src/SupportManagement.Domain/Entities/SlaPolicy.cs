using SupportManagement.Domain.Common;
using SupportManagement.Domain.Enums;

namespace SupportManagement.Domain.Entities;

public class SlaPolicy : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public TicketPriority Priority { get; set; }
    public Guid? CategoryId { get; set; }
    public int ResponseTimeMinutes { get; set; }
    public int ResolutionTimeMinutes { get; set; }
    public bool PauseDuringPendingUser { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public TicketCategory? Category { get; set; }
    public ICollection<SlaTrackingLog> TrackingLogs { get; set; } = new List<SlaTrackingLog>();
}
