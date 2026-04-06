using SupportManagement.Domain.Enums;

namespace SupportManagement.Application.DTOs;

public record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    string? ActionUrl,
    string? EntityType,
    Guid? EntityId,
    bool IsRead,
    DateTime? ReadAt,
    NotificationChannel Channel,
    DateTime CreatedAt
);
