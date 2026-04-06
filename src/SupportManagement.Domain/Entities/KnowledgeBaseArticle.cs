using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class KnowledgeBaseArticle : BaseEntity
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public bool IsPublished { get; set; } = false;
    public Guid CreatedById { get; set; }
    public Guid? SourceTicketId { get; set; }
    public int ViewCount { get; set; } = 0;

    public KnowledgeBaseCategory Category { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
}
