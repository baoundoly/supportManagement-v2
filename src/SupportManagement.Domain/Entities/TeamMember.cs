using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class TeamMember : BaseEntity
{
    public Guid TeamId { get; set; }
    public Guid UserId { get; set; }
    public bool IsLead { get; set; } = false;

    public SupportTeam Team { get; set; } = null!;
    public User User { get; set; } = null!;
}
