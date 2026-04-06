using SupportManagement.Domain.Enums;

namespace SupportManagement.Application.DTOs;

public record ChatRoomDto(
    Guid Id,
    Guid TicketId,
    ChatRoomType RoomType,
    bool IsInternal,
    DateTime CreatedAt
);

public record ChatMessageDto(
    Guid Id,
    Guid ChatRoomId,
    Guid SenderUserId,
    string SenderName,
    string? SenderAvatarUrl,
    string MessageText,
    string? AttachmentPath,
    string? AttachmentName,
    bool IsEdited,
    bool IsDeleted,
    DateTime SentAt,
    DateTime? EditedAt,
    int UnreadCount,
    bool IsReadByCurrentUser,
    List<ChatAttachmentDto> Attachments
);

public record SendMessageRequest(
    string MessageText,
    bool IsInternal = false
);

public record ChatAttachmentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSize,
    string FileUrl
);
