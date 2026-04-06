using SupportManagement.Domain.Enums;

namespace SupportManagement.Application.DTOs;

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string? Mobile,
    string? EmployeeId,
    string? Designation,
    UserRole Role,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? TeamId,
    string? TeamName,
    bool IsActive,
    bool IsOnline,
    DateTime? LastLoginAt,
    string? AvatarUrl,
    DateTime CreatedAt
);

public record CreateUserRequest(
    string FullName,
    string Email,
    string Password,
    string? Mobile,
    string? EmployeeId,
    string? Designation,
    UserRole Role,
    Guid? DepartmentId,
    Guid? TeamId
);

public record UpdateUserRequest(
    string FullName,
    string? Mobile,
    string? EmployeeId,
    string? Designation,
    UserRole Role,
    Guid? DepartmentId,
    Guid? TeamId
);

public record UpdateProfileRequest(
    string FullName,
    string? Mobile,
    string? Designation
);
