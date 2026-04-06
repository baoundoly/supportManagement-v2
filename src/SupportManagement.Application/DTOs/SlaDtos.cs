using SupportManagement.Domain.Enums;

namespace SupportManagement.Application.DTOs;

public record SlaPolicyDto(
    Guid Id,
    string Name,
    TicketPriority Priority,
    Guid? CategoryId,
    string? CategoryName,
    int ResponseTimeMinutes,
    int ResolutionTimeMinutes,
    bool PauseDuringPendingUser,
    bool IsActive
);

public record CreateSlaPolicyRequest(
    string Name,
    TicketPriority Priority,
    Guid? CategoryId,
    int ResponseTimeMinutes,
    int ResolutionTimeMinutes,
    bool PauseDuringPendingUser = true
);

public record UpdateSlaPolicyRequest(
    string Name,
    int ResponseTimeMinutes,
    int ResolutionTimeMinutes,
    bool PauseDuringPendingUser,
    bool IsActive
);
