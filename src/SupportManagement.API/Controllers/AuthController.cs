using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportManagement.Application.DTOs;
using SupportManagement.Application.Interfaces;
using SupportManagement.Domain.Entities;
using SupportManagement.Infrastructure.Persistence;

namespace SupportManagement.API.Controllers;

public class AuthController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthController(ApplicationDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Team)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenStr = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenStr,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        var userDto = MapUserDto(user);
        return Ok(new LoginResponse(accessToken, refreshTokenStr, userDto));
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var token = await _context.RefreshTokens
            .Include(t => t.User).ThenInclude(u => u.Department)
            .Include(t => t.User).ThenInclude(u => u.Team)
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken && !t.IsRevoked);

        if (token == null || token.ExpiresAt < DateTime.UtcNow)
            return Unauthorized(new { message = "Invalid or expired refresh token." });

        if (!token.User.IsActive)
            return Unauthorized(new { message = "User account is inactive." });

        var newAccessToken = _tokenService.GenerateAccessToken(token.User);
        var newRefreshTokenStr = _tokenService.GenerateRefreshToken();

        token.IsRevoked = true;
        token.ReplacedByToken = newRefreshTokenStr;
        token.RevokedAt = DateTime.UtcNow;
        token.ReasonRevoked = "Replaced by new token";

        var newRefreshToken = new RefreshToken
        {
            UserId = token.UserId,
            Token = newRefreshTokenStr,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        return Ok(new RefreshTokenResponse(newAccessToken, newRefreshTokenStr));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == request.RefreshToken && t.UserId == CurrentUserId);
        if (token != null)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.ReasonRevoked = "User logout";
            await _context.SaveChangesAsync();
        }
        return Ok(new { message = "Logged out successfully." });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Team)
            .FirstOrDefaultAsync(u => u.Id == CurrentUserId);

        if (user == null) return NotFound();
        return Ok(MapUserDto(user));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(CurrentUserId);
        if (user == null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return BadRequest(new { message = "Current password is incorrect." });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Password changed successfully." });
    }

    private static UserDto MapUserDto(User user) => new(
        user.Id, user.FullName, user.Email, user.Mobile, user.EmployeeId,
        user.Designation, user.Role, user.DepartmentId, user.Department?.Name,
        user.TeamId, user.Team?.Name, user.IsActive, user.IsOnline,
        user.LastLoginAt, user.AvatarUrl, user.CreatedAt
    );
}
