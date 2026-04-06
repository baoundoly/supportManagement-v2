using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class ChatMessageRead : BaseEntity
{
    public Guid ChatMessageId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    public ChatMessage ChatMessage { get; set; } = null!;
    public User User { get; set; } = null!;
}
