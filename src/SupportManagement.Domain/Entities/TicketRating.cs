using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class TicketRating : BaseEntity
{
    public Guid TicketId { get; set; }
    public Guid RatedById { get; set; }
    public int Rating { get; set; }
    public string? Feedback { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public User RatedBy { get; set; } = null!;
}
