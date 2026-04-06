using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid ChatRoomId { get; set; }
    public Guid SenderUserId { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public string? AttachmentPath { get; set; }
    public string? AttachmentName { get; set; }
    public bool IsEdited { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }

    public ChatRoom ChatRoom { get; set; } = null!;
    public User SenderUser { get; set; } = null!;
    public ICollection<ChatMessageRead> MessageReads { get; set; } = new List<ChatMessageRead>();
    public ICollection<ChatAttachment> Attachments { get; set; } = new List<ChatAttachment>();
}
