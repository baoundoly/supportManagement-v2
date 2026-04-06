using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<SupportTeam> SupportTeams { get; set; } = new List<SupportTeam>();
}
