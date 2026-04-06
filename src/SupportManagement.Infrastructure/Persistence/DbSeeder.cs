using Microsoft.EntityFrameworkCore;
using SupportManagement.Domain.Constants;
using SupportManagement.Domain.Entities;
using SupportManagement.Domain.Enums;

namespace SupportManagement.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Users.AnyAsync())
        {
            // Seed departments
            var dept = new Department { Name = "IT Support", Description = "Information Technology Support" };
            context.Departments.Add(dept);
            await context.SaveChangesAsync();

            // Seed support team
            var team = new SupportTeam { Name = "Level 1 Support", DepartmentId = dept.Id };
            context.SupportTeams.Add(team);
            await context.SaveChangesAsync();

            // Seed admin user
            var admin = new User
            {
                FullName = "System Admin",
                Email = "admin@support.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin,
                DepartmentId = dept.Id,
                IsActive = true
            };
            context.Users.Add(admin);

            // Seed agent
            var agent = new User
            {
                FullName = "Support Agent",
                Email = "agent@support.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Agent@123"),
                Role = UserRole.SupportAgent,
                DepartmentId = dept.Id,
                TeamId = team.Id,
                IsActive = true
            };
            context.Users.Add(agent);

            // Seed end user
            var endUser = new User
            {
                FullName = "John Doe",
                Email = "user@support.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                Role = UserRole.EndUser,
                IsActive = true
            };
            context.Users.Add(endUser);

            await context.SaveChangesAsync();

            // Add agent to team
            var teamMember = new TeamMember { TeamId = team.Id, UserId = agent.Id };
            context.TeamMembers.Add(teamMember);
            await context.SaveChangesAsync();

            // Seed categories
            var categories = new[]
            {
                new TicketCategory { Name = "Hardware", Description = "Hardware-related issues" },
                new TicketCategory { Name = "Software", Description = "Software-related issues" },
                new TicketCategory { Name = "Network", Description = "Network-related issues" },
                new TicketCategory { Name = "Account", Description = "Account and access issues" },
                new TicketCategory { Name = "Other", Description = "Other issues" }
            };
            context.TicketCategories.AddRange(categories);
            await context.SaveChangesAsync();

            // Seed subcategories
            var subcategories = new[]
            {
                new TicketSubcategory { CategoryId = categories[0].Id, Name = "Laptop/Desktop" },
                new TicketSubcategory { CategoryId = categories[0].Id, Name = "Printer" },
                new TicketSubcategory { CategoryId = categories[1].Id, Name = "OS Issues" },
                new TicketSubcategory { CategoryId = categories[1].Id, Name = "Application Error" },
                new TicketSubcategory { CategoryId = categories[2].Id, Name = "Internet Connectivity" },
                new TicketSubcategory { CategoryId = categories[2].Id, Name = "VPN Issues" },
                new TicketSubcategory { CategoryId = categories[3].Id, Name = "Password Reset" },
                new TicketSubcategory { CategoryId = categories[3].Id, Name = "Access Request" }
            };
            context.TicketSubcategories.AddRange(subcategories);
            await context.SaveChangesAsync();

            // Seed SLA policies
            var slaPolicies = new[]
            {
                new SlaPolicy { Name = "Critical SLA", Priority = TicketPriority.Critical, ResponseTimeMinutes = AppConstants.SlaDefaults.CriticalResponseMinutes, ResolutionTimeMinutes = AppConstants.SlaDefaults.CriticalResolutionMinutes },
                new SlaPolicy { Name = "High SLA", Priority = TicketPriority.High, ResponseTimeMinutes = AppConstants.SlaDefaults.HighResponseMinutes, ResolutionTimeMinutes = AppConstants.SlaDefaults.HighResolutionMinutes },
                new SlaPolicy { Name = "Medium SLA", Priority = TicketPriority.Medium, ResponseTimeMinutes = AppConstants.SlaDefaults.MediumResponseMinutes, ResolutionTimeMinutes = AppConstants.SlaDefaults.MediumResolutionMinutes },
                new SlaPolicy { Name = "Low SLA", Priority = TicketPriority.Low, ResponseTimeMinutes = AppConstants.SlaDefaults.LowResponseMinutes, ResolutionTimeMinutes = AppConstants.SlaDefaults.LowResolutionMinutes }
            };
            context.SlaPolicies.AddRange(slaPolicies);
            await context.SaveChangesAsync();
        }
    }
}
