using SupportManagement.Domain.Enums;

namespace SupportManagement.Application.DTOs;

public record TicketDto(
    Guid Id,
    string TicketNo,
    string Subject,
    string Description,
    Guid CategoryId,
    string CategoryName,
    Guid? SubcategoryId,
    string? SubcategoryName,
    TicketPriority Priority,
    TicketStatus Status,
    Guid CreatedById,
    string CreatedByName,
    Guid? AssignedTeamId,
    string? AssignedTeamName,
    Guid? AssignedUserId,
    string? AssignedUserName,
    string? AffectedModule,
    string? ReportedSource,
    DateTime? FirstResponseDueAt,
    DateTime? ResolutionDueAt,
    DateTime? FirstResponseAt,
    DateTime? ResolvedAt,
    DateTime? ClosedAt,
    int ReopenCount,
    bool IsSlaBreached,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<TicketAttachmentDto> Attachments
);

public record TicketSummaryDto(
    Guid Id,
    string TicketNo,
    string Subject,
    TicketPriority Priority,
    TicketStatus Status,
    string CategoryName,
    string? AssignedUserName,
    string? AssignedTeamName,
    bool IsSlaBreached,
    DateTime? ResolutionDueAt,
    DateTime CreatedAt
);

public record CreateTicketRequest(
    string Subject,
    string Description,
    Guid CategoryId,
    Guid? SubcategoryId,
    TicketPriority Priority,
    string? AffectedModule,
    string? ReportedSource
);

public record UpdateTicketRequest(
    string Subject,
    string Description,
    Guid CategoryId,
    Guid? SubcategoryId,
    TicketPriority Priority,
    string? AffectedModule
);

public record AssignTicketRequest(
    Guid? AssignedToUserId,
    Guid? AssignedToTeamId,
    string? Remarks
);

public record ChangeStatusRequest(
    TicketStatus NewStatus,
    string? Remarks
);

public record ResolveTicketRequest(
    string RootCause,
    string ResolutionSummary,
    string? FixType,
    string? ClosureNotes
);

public record ReopenTicketRequest(string Reason);

public record TicketAttachmentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSize,
    string FileUrl,
    DateTime CreatedAt
);

public record TicketFilterRequest(
    TicketStatus? Status,
    TicketPriority? Priority,
    Guid? CategoryId,
    Guid? AssignedUserId,
    Guid? AssignedTeamId,
    bool? IsSlaBreached,
    DateTime? FromDate,
    DateTime? ToDate,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20
);

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
