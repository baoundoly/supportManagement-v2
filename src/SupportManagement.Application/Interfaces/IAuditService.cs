namespace SupportManagement.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(string actionType, string entityName, Guid? entityId = null, Guid? performedById = null, string? oldValue = null, string? newValue = null, string? ipAddress = null, CancellationToken cancellationToken = default);
}
