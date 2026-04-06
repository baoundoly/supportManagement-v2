using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class TicketSubcategory : BaseEntity
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public TicketCategory Category { get; set; } = null!;
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
