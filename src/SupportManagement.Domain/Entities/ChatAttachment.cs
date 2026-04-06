using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class ChatAttachment : BaseEntity
{
    public Guid ChatMessageId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;

    public ChatMessage ChatMessage { get; set; } = null!;
}
