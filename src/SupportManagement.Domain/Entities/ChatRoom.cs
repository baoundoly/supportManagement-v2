using SupportManagement.Domain.Common;
using SupportManagement.Domain.Enums;

namespace SupportManagement.Domain.Entities;

public class ChatRoom : BaseEntity
{
    public Guid TicketId { get; set; }
    public ChatRoomType RoomType { get; set; } = ChatRoomType.Public;
    public bool IsInternal { get; set; } = false;
    public Guid CreatedById { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
}
