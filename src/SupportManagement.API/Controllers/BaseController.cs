using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SupportManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
        }
    }

    protected string CurrentUserName => User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    protected string CurrentUserRole => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    protected string? CurrentUserIp => HttpContext.Connection.RemoteIpAddress?.ToString();
}
