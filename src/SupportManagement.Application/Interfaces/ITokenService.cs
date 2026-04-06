using SupportManagement.Domain.Entities;

namespace SupportManagement.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? ValidateAccessToken(string token);
}
