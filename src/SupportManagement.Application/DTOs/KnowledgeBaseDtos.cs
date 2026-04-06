namespace SupportManagement.Application.DTOs;

public record KnowledgeBaseArticleDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Content,
    string? Tags,
    bool IsPublished,
    Guid CreatedById,
    string CreatedByName,
    Guid? SourceTicketId,
    int ViewCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateArticleRequest(
    Guid CategoryId,
    string Title,
    string Content,
    string? Tags,
    bool IsPublished = false,
    Guid? SourceTicketId = null
);

public record UpdateArticleRequest(
    string Title,
    string Content,
    string? Tags,
    bool IsPublished
);

public record KnowledgeBaseCategoryDto(Guid Id, string Name, string? Description, bool IsActive, int ArticleCount);
