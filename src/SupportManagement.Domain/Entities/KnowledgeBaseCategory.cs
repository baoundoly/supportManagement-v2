using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class KnowledgeBaseCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<KnowledgeBaseArticle> Articles { get; set; } = new List<KnowledgeBaseArticle>();
}
