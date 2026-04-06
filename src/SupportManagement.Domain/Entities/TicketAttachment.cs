using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class TicketAttachment : BaseEntity
{
    public Guid TicketId { get; set; }
    public Guid UploadedById { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;

    public Ticket Ticket { get; set; } = null!;
    public User UploadedBy { get; set; } = null!;
}
