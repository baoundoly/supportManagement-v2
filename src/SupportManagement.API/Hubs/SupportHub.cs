using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SupportManagement.Domain.Enums;
using SupportManagement.Infrastructure.Persistence;

namespace SupportManagement.API.Hubs;

[Authorize]
public class SupportHub : Hub
{
    private readonly ApplicationDbContext _context;

    public SupportHub(ApplicationDbContext context)
    {
        _context = context;
    }

    private Guid CurrentUserId => Guid.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? Context.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
        ?? Guid.Empty.ToString());

    private string CurrentUserRole => Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    public override async Task OnConnectedAsync()
    {
        var userId = CurrentUserId;
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.IsOnline = true;
            await _context.SaveChangesAsync();
            await Clients.All.SendAsync("PresenceUpdated", userId, true);
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = CurrentUserId;
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.IsOnline = false;
            await _context.SaveChangesAsync();
            await Clients.All.SendAsync("PresenceUpdated", userId, false);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinTicketRoom(string ticketId, string roomType)
    {
        var isInternal = roomType.ToLower() == "internal";
        if (isInternal && CurrentUserRole == UserRole.EndUser.ToString())
            throw new HubException("Access denied to internal chat room.");

        var groupName = $"ticket_{ticketId}_{roomType}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveTicketRoom(string ticketId, string roomType)
    {
        var groupName = $"ticket_{ticketId}_{roomType}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SendPublicMessage(string ticketId, string messageText)
    {
        await BroadcastMessage(ticketId, "public", messageText);
    }

    public async Task SendInternalMessage(string ticketId, string messageText)
    {
        if (CurrentUserRole == UserRole.EndUser.ToString())
            throw new HubException("End users cannot send internal messages.");

        await BroadcastMessage(ticketId, "internal", messageText);
    }

    public async Task Typing(string ticketId, string roomType)
    {
        var groupName = $"ticket_{ticketId}_{roomType}";
        var userId = CurrentUserId;
        var user = await _context.Users.FindAsync(userId);
        await Clients.OthersInGroup(groupName).SendAsync("UserTyping", ticketId, roomType, userId, user?.FullName);
    }

    public async Task MarkMessageRead(string messageId)
    {
        if (Guid.TryParse(messageId, out var msgId))
        {
            var userId = CurrentUserId;
            var alreadyRead = await _context.ChatMessageReads.AnyAsync(r => r.ChatMessageId == msgId && r.UserId == userId);
            if (!alreadyRead)
            {
                _context.ChatMessageReads.Add(new Domain.Entities.ChatMessageRead
                {
                    ChatMessageId = msgId,
                    UserId = userId
                });
                await _context.SaveChangesAsync();
            }

            var msg = await _context.ChatMessages.Include(m => m.ChatRoom).FirstOrDefaultAsync(m => m.Id == msgId);
            if (msg != null)
            {
                var groupName = $"ticket_{msg.ChatRoom.TicketId}_{(msg.ChatRoom.IsInternal ? "internal" : "public")}";
                await Clients.Group(groupName).SendAsync("MessageRead", messageId, userId);
            }
        }
    }

    private async Task BroadcastMessage(string ticketId, string roomType, string messageText)
    {
        var groupName = $"ticket_{ticketId}_{roomType}";
        var userId = CurrentUserId;
        var user = await _context.Users.FindAsync(userId);

        var payload = new
        {
            SenderId = userId,
            SenderName = user?.FullName,
            SenderAvatarUrl = user?.AvatarUrl,
            MessageText = messageText,
            SentAt = DateTime.UtcNow,
            TicketId = ticketId,
            RoomType = roomType
        };

        await Clients.Group(groupName).SendAsync("ReceiveMessage", payload);
    }
}
