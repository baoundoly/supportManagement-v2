using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class TicketResolution : BaseEntity
{
    public Guid TicketId { get; set; }
    public string RootCause { get; set; } = string.Empty;
    public string ResolutionSummary { get; set; } = string.Empty;
    public string? FixType { get; set; }
    public DateTime ResolutionDate { get; set; } = DateTime.UtcNow;
    public Guid ResolvedById { get; set; }
    public bool UserConfirmed { get; set; } = false;
    public string? ReopenReason { get; set; }
    public string? ClosureNotes { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public User ResolvedBy { get; set; } = null!;
}
