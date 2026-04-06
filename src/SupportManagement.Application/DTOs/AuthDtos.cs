namespace SupportManagement.Application.DTOs;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string AccessToken, string RefreshToken, UserDto User);
public record RefreshTokenRequest(string RefreshToken);
public record RefreshTokenResponse(string AccessToken, string RefreshToken);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
