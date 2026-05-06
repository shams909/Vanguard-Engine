using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Ensure database is created and migrations applied
        await context.Database.MigrateAsync();

        // Seed Roles
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new Role { RoleName = "Admin", Description = "System Administrator" },
                new Role { RoleName = "Guard", Description = "Security Guard Applicant" },
                new Role { RoleName = "Client", Description = "Recruiter/Client" }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // Seed Admin User
        if (!await context.Users.AnyAsync(u => u.Username == "admin"))
        {
            var adminRole = await context.Roles.FirstAsync(r => r.RoleName == "Admin");
            var hasher = new PasswordHasher<User>();
            
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@vanguard.com",
                Address = "Vanguard Headquarters",
                RoleId = adminRole.Id,
                LastLogin = DateTime.UtcNow
            };

            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin123!");

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }
    }
}
