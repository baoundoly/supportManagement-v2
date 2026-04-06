using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SupportManagement.Application.Interfaces;
using SupportManagement.Infrastructure.Identity;
using SupportManagement.Infrastructure.Persistence;
using SupportManagement.Infrastructure.Services;
using SupportManagement.Infrastructure.Storage;

namespace SupportManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // JWT
        var jwtSection = configuration.GetSection("JwtSettings");
        var jwtSettings = jwtSection.Get<JwtSettings>();
        if (jwtSettings is null || string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
            throw new InvalidOperationException("JWT configuration is missing or invalid. Ensure 'JwtSettings:SecretKey' is configured.");

        services.Configure<JwtSettings>(jwtSection);
        services.AddScoped<ITokenService, TokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ClockSkew = TimeSpan.Zero
                };

                // SignalR authentication
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        // Services
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ReportService>();

        return services;
    }
}
