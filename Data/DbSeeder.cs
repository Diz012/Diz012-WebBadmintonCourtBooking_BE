using CourtBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CourtBookingAPI.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Seed Owner if not exists
            if (!await context.Users.AnyAsync(u => u.Email == "owner@example.com"))
            {
                var owner = new User
                {
                    FullName = "Owner",
                    Email = "owner@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Owner123"),
                    Role = "Owner"
                };
                context.Users.Add(owner);
                await context.SaveChangesAsync();
            }

            // Seed Admin if not exists
            if (!await context.Users.AnyAsync(u => u.Email == "admin@example.com"))
            {
                var admin = new User
                {
                    FullName = "Admin",
                    Email = "admin@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                    Role = "Admin"
                };
                context.Users.Add(admin);
                await context.SaveChangesAsync();
            }
        }
    }
}
