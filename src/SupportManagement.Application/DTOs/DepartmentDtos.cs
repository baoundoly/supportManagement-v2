namespace SupportManagement.Application.DTOs;

public record DepartmentDto(Guid Id, string Name, string? Description, bool IsActive, DateTime CreatedAt);
public record CreateDepartmentRequest(string Name, string? Description);
public record SupportTeamDto(Guid Id, string Name, string? Description, Guid? DepartmentId, string? DepartmentName, bool IsActive, int MemberCount);
public record CreateSupportTeamRequest(string Name, string? Description, Guid? DepartmentId);
