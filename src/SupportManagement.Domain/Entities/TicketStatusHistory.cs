using SupportManagement.Domain.Common;
using SupportManagement.Domain.Enums;

namespace SupportManagement.Domain.Entities;

public class TicketStatusHistory : BaseEntity
{
    public Guid TicketId { get; set; }
    public TicketStatus OldStatus { get; set; }
    public TicketStatus NewStatus { get; set; }
    public Guid ChangedById { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? Remarks { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public User ChangedBy { get; set; } = null!;
}
