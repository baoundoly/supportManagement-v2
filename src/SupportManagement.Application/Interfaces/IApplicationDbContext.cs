using Microsoft.EntityFrameworkCore;
using SupportManagement.Domain.Entities;

namespace SupportManagement.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Department> Departments { get; }
    DbSet<SupportTeam> SupportTeams { get; }
    DbSet<TeamMember> TeamMembers { get; }
    DbSet<Ticket> Tickets { get; }
    DbSet<TicketCategory> TicketCategories { get; }
    DbSet<TicketSubcategory> TicketSubcategories { get; }
    DbSet<TicketAttachment> TicketAttachments { get; }
    DbSet<TicketAssignment> TicketAssignments { get; }
    DbSet<TicketStatusHistory> TicketStatusHistories { get; }
    DbSet<TicketResolution> TicketResolutions { get; }
    DbSet<TicketEscalation> TicketEscalations { get; }
    DbSet<TicketRating> TicketRatings { get; }
    DbSet<ChatRoom> ChatRooms { get; }
    DbSet<ChatParticipant> ChatParticipants { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<ChatMessageRead> ChatMessageReads { get; }
    DbSet<ChatAttachment> ChatAttachments { get; }
    DbSet<SlaPolicy> SlaPolicies { get; }
    DbSet<SlaTrackingLog> SlaTrackingLogs { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<KnowledgeBaseCategory> KnowledgeBaseCategories { get; }
    DbSet<KnowledgeBaseArticle> KnowledgeBaseArticles { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
