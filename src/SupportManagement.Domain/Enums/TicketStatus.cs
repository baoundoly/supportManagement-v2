namespace SupportManagement.Domain.Enums;

public enum TicketStatus
{
    New = 1,
    Open = 2,
    Assigned = 3,
    InProgress = 4,
    PendingUser = 5,
    PendingVendor = 6,
    Resolved = 7,
    Closed = 8,
    Reopened = 9,
    Cancelled = 10
}
