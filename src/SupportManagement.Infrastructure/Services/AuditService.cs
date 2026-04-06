using SupportManagement.Application.Interfaces;
using SupportManagement.Domain.Entities;
using SupportManagement.Infrastructure.Persistence;

namespace SupportManagement.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string actionType, string entityName, Guid? entityId = null, Guid? performedById = null, string? oldValue = null, string? newValue = null, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            ActionType = actionType,
            EntityName = entityName,
            EntityId = entityId,
            PerformedById = performedById,
            OldValue = oldValue,
            NewValue = newValue,
            IpAddress = ipAddress
        };
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
