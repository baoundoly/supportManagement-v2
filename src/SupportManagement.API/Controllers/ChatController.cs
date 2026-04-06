using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportManagement.Application.DTOs;
using SupportManagement.Application.Interfaces;
using SupportManagement.Domain.Entities;
using SupportManagement.Domain.Enums;
using SupportManagement.Infrastructure.Persistence;

namespace SupportManagement.API.Controllers;

[Authorize]
public class ChatController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public ChatController(ApplicationDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    [HttpGet("tickets/{ticketId}/chat/{roomType}")]
    public async Task<IActionResult> GetMessages(Guid ticketId, string roomType, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var isInternal = roomType.ToLower() == "internal";

        if (isInternal && CurrentUserRole == UserRole.EndUser.ToString())
            return Forbid();

        var room = await _context.ChatRooms
            .FirstOrDefaultAsync(r => r.TicketId == ticketId && r.IsInternal == isInternal);

        if (room == null)
        {
            room = new ChatRoom
            {
                TicketId = ticketId,
                RoomType = isInternal ? ChatRoomType.Internal : ChatRoomType.Public,
                IsInternal = isInternal,
                CreatedById = CurrentUserId
            };
            _context.ChatRooms.Add(room);
            await _context.SaveChangesAsync();
        }

        var messages = await _context.ChatMessages
            .Include(m => m.SenderUser)
            .Include(m => m.MessageReads)
            .Include(m => m.Attachments)
            .Where(m => m.ChatRoomId == room.Id && !m.IsDeleted)
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = messages.Select(m => MapMessageDto(m, CurrentUserId)).ToList();
        return Ok(dtos);
    }

    [HttpPost("tickets/{ticketId}/chat/{roomType}/message")]
    public async Task<IActionResult> SendMessage(Guid ticketId, string roomType, [FromBody] SendMessageRequest request)
    {
        var isInternal = roomType.ToLower() == "internal";

        if (isInternal && CurrentUserRole == UserRole.EndUser.ToString())
            return Forbid();

        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) return NotFound();

        if (ticket.Status == TicketStatus.Closed || ticket.Status == TicketStatus.Cancelled)
            return BadRequest(new { message = "Cannot send messages on a closed or cancelled ticket." });

        var room = await _context.ChatRooms
            .FirstOrDefaultAsync(r => r.TicketId == ticketId && r.IsInternal == isInternal);

        if (room == null)
        {
            room = new ChatRoom
            {
                TicketId = ticketId,
                RoomType = isInternal ? ChatRoomType.Internal : ChatRoomType.Public,
                IsInternal = isInternal,
                CreatedById = CurrentUserId
            };
            _context.ChatRooms.Add(room);
            await _context.SaveChangesAsync();
        }

        if (ticket.FirstResponseAt == null && !isInternal)
        {
            ticket.FirstResponseAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;
        }

        var message = new ChatMessage
        {
            ChatRoomId = room.Id,
            SenderUserId = CurrentUserId,
            MessageText = request.MessageText,
            SentAt = DateTime.UtcNow
        };
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        var saved = await _context.ChatMessages
            .Include(m => m.SenderUser)
            .Include(m => m.MessageReads)
            .Include(m => m.Attachments)
            .FirstAsync(m => m.Id == message.Id);

        return Ok(MapMessageDto(saved, CurrentUserId));
    }

    [HttpPost("messages/{messageId}/read")]
    public async Task<IActionResult> MarkRead(Guid messageId)
    {
        var exists = await _context.ChatMessageReads.AnyAsync(r => r.ChatMessageId == messageId && r.UserId == CurrentUserId);
        if (!exists)
        {
            _context.ChatMessageReads.Add(new ChatMessageRead
            {
                ChatMessageId = messageId,
                UserId = CurrentUserId
            });
            await _context.SaveChangesAsync();
        }
        return Ok();
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = CurrentUserId;
        var userRole = CurrentUserRole;

        var query = _context.ChatMessages
            .Include(m => m.ChatRoom)
            .Where(m => !m.IsDeleted && m.SenderUserId != userId);

        if (userRole == UserRole.EndUser.ToString())
            query = query.Where(m => !m.ChatRoom.IsInternal);

        var count = await query
            .Where(m => !m.MessageReads.Any(r => r.UserId == userId))
            .CountAsync();

        return Ok(new { count });
    }

    [HttpPost("tickets/{ticketId}/chat/attachment")]
    public async Task<IActionResult> UploadChatAttachment(Guid ticketId, [FromForm] string roomType, IFormFile file, [FromForm] string? messageText)
    {
        var isInternal = roomType.ToLower() == "internal";

        if (isInternal && CurrentUserRole == UserRole.EndUser.ToString())
            return Forbid();

        if (file == null || file.Length == 0) return BadRequest(new { message = "No file provided." });

        var room = await _context.ChatRooms
            .FirstOrDefaultAsync(r => r.TicketId == ticketId && r.IsInternal == isInternal);

        if (room == null)
        {
            room = new ChatRoom
            {
                TicketId = ticketId,
                RoomType = isInternal ? ChatRoomType.Internal : ChatRoomType.Public,
                IsInternal = isInternal,
                CreatedById = CurrentUserId
            };
            _context.ChatRooms.Add(room);
            await _context.SaveChangesAsync();
        }

        await using var stream = file.OpenReadStream();
        var (storedFileName, filePath) = await _fileStorage.SaveFileAsync(stream, file.FileName, $"chat/{ticketId}");

        var message = new ChatMessage
        {
            ChatRoomId = room.Id,
            SenderUserId = CurrentUserId,
            MessageText = messageText ?? string.Empty,
            AttachmentPath = filePath,
            AttachmentName = file.FileName,
            SentAt = DateTime.UtcNow
        };
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        var attachment = new ChatAttachment
        {
            ChatMessageId = message.Id,
            FileName = file.FileName,
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            FilePath = filePath
        };
        _context.ChatAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        return Ok(new { messageId = message.Id, fileUrl = _fileStorage.GetFileUrl(filePath) });
    }

    private ChatMessageDto MapMessageDto(ChatMessage m, Guid currentUserId) => new(
        m.Id, m.ChatRoomId, m.SenderUserId, m.SenderUser?.FullName ?? string.Empty,
        m.SenderUser?.AvatarUrl, m.MessageText, m.AttachmentPath, m.AttachmentName,
        m.IsEdited, m.IsDeleted, m.SentAt, m.EditedAt,
        0, m.MessageReads.Any(r => r.UserId == currentUserId),
        m.Attachments.Select(a => new ChatAttachmentDto(
            a.Id, a.FileName, a.ContentType, a.FileSize,
            _fileStorage.GetFileUrl(a.FilePath))).ToList()
    );
}
