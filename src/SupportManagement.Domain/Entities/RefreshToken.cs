using SupportManagement.Domain.Common;

namespace SupportManagement.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    public User User { get; set; } = null!;
}
