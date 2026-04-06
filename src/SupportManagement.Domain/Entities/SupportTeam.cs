using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class SupportTeam : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;

    public Department? Department { get; set; }
    public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
}
