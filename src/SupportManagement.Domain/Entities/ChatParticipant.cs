using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class ChatParticipant : BaseEntity
{
    public Guid ChatRoomId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public ChatRoom ChatRoom { get; set; } = null!;
    public User User { get; set; } = null!;
}
