using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportManagement.Application.DTOs;
using SupportManagement.Domain.Entities;
using SupportManagement.Infrastructure.Persistence;

namespace SupportManagement.API.Controllers;

[Authorize]
public class KnowledgeBaseController : BaseController
{
    private readonly ApplicationDbContext _context;

    public KnowledgeBaseController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("articles")]
    [AllowAnonymous]
    public async Task<IActionResult> GetArticles([FromQuery] string? search, [FromQuery] Guid? categoryId, [FromQuery] bool publishedOnly = true)
    {
        var query = _context.KnowledgeBaseArticles
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .AsQueryable();

        if (publishedOnly) query = query.Where(a => a.IsPublished);
        if (categoryId.HasValue) query = query.Where(a => a.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.Title.Contains(search) || a.Content.Contains(search) || (a.Tags != null && a.Tags.Contains(search)));

        var articles = await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new KnowledgeBaseArticleDto(
                a.Id, a.CategoryId, a.Category.Name, a.Title, a.Content, a.Tags,
                a.IsPublished, a.CreatedById, a.CreatedBy.FullName, a.SourceTicketId, a.ViewCount,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync();

        return Ok(articles);
    }

    [HttpGet("articles/{id}")]
    public async Task<IActionResult> GetArticle(Guid id)
    {
        var article = await _context.KnowledgeBaseArticles
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article == null) return NotFound();

        article.ViewCount++;
        await _context.SaveChangesAsync();

        return Ok(new KnowledgeBaseArticleDto(
            article.Id, article.CategoryId, article.Category?.Name ?? string.Empty,
            article.Title, article.Content, article.Tags, article.IsPublished,
            article.CreatedById, article.CreatedBy?.FullName ?? string.Empty,
            article.SourceTicketId, article.ViewCount, article.CreatedAt, article.UpdatedAt));
    }

    [HttpPost("articles")]
    [Authorize(Roles = "Admin,TeamLead,SupportAgent")]
    public async Task<IActionResult> CreateArticle([FromBody] CreateArticleRequest request)
    {
        var article = new KnowledgeBaseArticle
        {
            CategoryId = request.CategoryId,
            Title = request.Title,
            Content = request.Content,
            Tags = request.Tags,
            IsPublished = request.IsPublished,
            CreatedById = CurrentUserId,
            SourceTicketId = request.SourceTicketId
        };
        _context.KnowledgeBaseArticles.Add(article);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetArticle), new { id = article.Id }, new { id = article.Id });
    }

    [HttpPut("articles/{id}")]
    [Authorize(Roles = "Admin,TeamLead,SupportAgent")]
    public async Task<IActionResult> UpdateArticle(Guid id, [FromBody] UpdateArticleRequest request)
    {
        var article = await _context.KnowledgeBaseArticles.FindAsync(id);
        if (article == null) return NotFound();

        article.Title = request.Title;
        article.Content = request.Content;
        article.Tags = request.Tags;
        article.IsPublished = request.IsPublished;
        article.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Article updated." });
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.KnowledgeBaseCategories
            .Include(c => c.Articles)
            .Select(c => new KnowledgeBaseCategoryDto(c.Id, c.Name, c.Description, c.IsActive, c.Articles.Count(a => a.IsPublished)))
            .ToListAsync();
        return Ok(categories);
    }

    [HttpPost("categories")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateDepartmentRequest request)
    {
        var cat = new KnowledgeBaseCategory { Name = request.Name, Description = request.Description };
        _context.KnowledgeBaseCategories.Add(cat);
        await _context.SaveChangesAsync();
        return Ok(new { id = cat.Id });
    }
}
