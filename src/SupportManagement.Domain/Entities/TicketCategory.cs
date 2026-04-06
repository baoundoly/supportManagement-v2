using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class TicketCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<TicketSubcategory> Subcategories { get; set; } = new List<TicketSubcategory>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<SlaPolicy> SlaPolicies { get; set; } = new List<SlaPolicy>();
}
