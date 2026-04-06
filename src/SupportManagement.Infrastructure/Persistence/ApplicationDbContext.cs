using Microsoft.EntityFrameworkCore;
using SupportManagement.Application.Interfaces;
using SupportManagement.Domain.Entities;
using SupportManagement.Domain.Enums;

namespace SupportManagement.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<SupportTeam> SupportTeams => Set<SupportTeam>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();
    public DbSet<TicketSubcategory> TicketSubcategories => Set<TicketSubcategory>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
    public DbSet<TicketAssignment> TicketAssignments => Set<TicketAssignment>();
    public DbSet<TicketStatusHistory> TicketStatusHistories => Set<TicketStatusHistory>();
    public DbSet<TicketResolution> TicketResolutions => Set<TicketResolution>();
    public DbSet<TicketEscalation> TicketEscalations => Set<TicketEscalation>();
    public DbSet<TicketRating> TicketRatings => Set<TicketRating>();
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<ChatParticipant> ChatParticipants => Set<ChatParticipant>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ChatMessageRead> ChatMessageReads => Set<ChatMessageRead>();
    public DbSet<ChatAttachment> ChatAttachments => Set<ChatAttachment>();
    public DbSet<SlaPolicy> SlaPolicies => Set<SlaPolicy>();
    public DbSet<SlaTrackingLog> SlaTrackingLogs => Set<SlaTrackingLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<KnowledgeBaseCategory> KnowledgeBaseCategories => Set<KnowledgeBaseCategory>();
    public DbSet<KnowledgeBaseArticle> KnowledgeBaseArticles => Set<KnowledgeBaseArticle>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<int>();
        });

        // Ticket
        modelBuilder.Entity<Ticket>(e =>
        {
            e.HasIndex(t => t.TicketNo).IsUnique();
            e.Property(t => t.Priority).HasConversion<int>();
            e.Property(t => t.Status).HasConversion<int>();
            e.HasOne(t => t.CreatedBy).WithMany(u => u.CreatedTickets).HasForeignKey(t => t.CreatedById).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.AssignedUser).WithMany().HasForeignKey(t => t.AssignedUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.AssignedTeam).WithMany(t => t.AssignedTickets).HasForeignKey(t => t.AssignedTeamId).OnDelete(DeleteBehavior.Restrict);
        });

        // TicketAssignment
        modelBuilder.Entity<TicketAssignment>(e =>
        {
            e.HasOne(a => a.AssignedToUser).WithMany().HasForeignKey(a => a.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.AssignedByUser).WithMany(u => u.Assignments).HasForeignKey(a => a.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.AssignedToTeam).WithMany().HasForeignKey(a => a.AssignedToTeamId).OnDelete(DeleteBehavior.Restrict);
        });

        // TicketStatusHistory
        modelBuilder.Entity<TicketStatusHistory>(e =>
        {
            e.Property(h => h.OldStatus).HasConversion<int>();
            e.Property(h => h.NewStatus).HasConversion<int>();
            e.HasOne(h => h.ChangedBy).WithMany().HasForeignKey(h => h.ChangedById).OnDelete(DeleteBehavior.Restrict);
        });

        // TicketEscalation
        modelBuilder.Entity<TicketEscalation>(e =>
        {
            e.HasOne(esc => esc.EscalatedBy).WithMany().HasForeignKey(esc => esc.EscalatedById).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(esc => esc.EscalatedToUser).WithMany().HasForeignKey(esc => esc.EscalatedToUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(esc => esc.EscalatedToTeam).WithMany().HasForeignKey(esc => esc.EscalatedToTeamId).OnDelete(DeleteBehavior.Restrict);
        });

        // ChatRoom
        modelBuilder.Entity<ChatRoom>(e =>
        {
            e.Property(r => r.RoomType).HasConversion<int>();
            e.HasOne(r => r.CreatedBy).WithMany().HasForeignKey(r => r.CreatedById).OnDelete(DeleteBehavior.Restrict);
        });

        // ChatMessage
        modelBuilder.Entity<ChatMessage>(e =>
        {
            e.HasOne(m => m.SenderUser).WithMany(u => u.ChatMessages).HasForeignKey(m => m.SenderUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ChatMessageRead
        modelBuilder.Entity<ChatMessageRead>(e =>
        {
            e.HasIndex(r => new { r.ChatMessageId, r.UserId }).IsUnique();
            e.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        // SlaPolicy
        modelBuilder.Entity<SlaPolicy>(e =>
        {
            e.Property(s => s.Priority).HasConversion<int>();
        });

        // Notification
        modelBuilder.Entity<Notification>(e =>
        {
            e.Property(n => n.Channel).HasConversion<int>();
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasOne(a => a.PerformedBy).WithMany(u => u.AuditLogs).HasForeignKey(a => a.PerformedById).OnDelete(DeleteBehavior.Restrict);
        });

        // TicketResolution
        modelBuilder.Entity<TicketResolution>(e =>
        {
            e.HasOne(r => r.ResolvedBy).WithMany().HasForeignKey(r => r.ResolvedById).OnDelete(DeleteBehavior.Restrict);
        });

        // TeamMember - composite unique constraint
        modelBuilder.Entity<TeamMember>(e =>
        {
            e.HasIndex(m => new { m.TeamId, m.UserId }).IsUnique();
        });
    }
}
