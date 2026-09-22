using Microsoft.AspNetCore.Identity;
using BugFlow.Web.Data;
using BugFlow.Web.Models;
using BugFlow.Web.Models.Enums;

namespace BugFlow.Web.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Roles
            string[] roleNames = { "Admin", "Tester", "Developer" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Default users (one per role)
            await CreateUserIfNotExistsAsync(userManager, "admin@bugflow.com", "Admin@123", "System Admin", "Admin");
            await CreateUserIfNotExistsAsync(userManager, "tester@bugflow.com", "Tester@123", "Default Tester", "Tester");
            await CreateUserIfNotExistsAsync(userManager, "dev@bugflow.com", "Dev@123", "Default Developer", "Developer");
        }

        private static async Task CreateUserIfNotExistsAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string fullName,
            string role)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null) return;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
            else
            {
                throw new Exception(
                    $"Failed to create user {email}: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        public static async Task SeedSampleBugsAsync(ApplicationDbContext context)
        {
            // Don't reseed if bugs already exist
            if (context.Bugs.Any()) return;

            var tester = context.Users.FirstOrDefault(u => u.Email == "tester@bugflow.com");
            var developer = context.Users.FirstOrDefault(u => u.Email == "dev@bugflow.com");

            if (tester == null || developer == null) return; // roles/users not seeded yet

            var bugs = new List<Bug>
            {
                new Bug
                {
                    Title = "Login button unresponsive on Safari",
                    Description = "Clicking the login button does nothing on Safari 17. Works fine on Chrome/Edge.",
                    Priority = BugPriority.High,
                    Status = BugStatus.New,
                    ReportedById = tester.Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-3)
                },
                new Bug
                {
                    Title = "Dashboard stats show wrong bug count",
                    Description = "Total bug count on dashboard doesn't match the actual number of rows in the Bugs table.",
                    Priority = BugPriority.Medium,
                    Status = BugStatus.Assigned,
                    ReportedById = tester.Id,
                    AssignedToId = developer.Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-2)
                },
                new Bug
                {
                    Title = "App crashes on null bug description",
                    Description = "Creating a bug with an empty description field throws a 500 error instead of a validation message.",
                    Priority = BugPriority.Critical,
                    Status = BugStatus.InProgress,
                    ReportedById = tester.Id,
                    AssignedToId = developer.Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-1)
                },
                new Bug
                {
                    Title = "Typo in navbar 'Dashbord'",
                    Description = "Minor spelling mistake in the top navigation bar.",
                    Priority = BugPriority.Low,
                    Status = BugStatus.Resolved,
                    ReportedById = tester.Id,
                    AssignedToId = developer.Id,
                    CreatedDate = DateTime.UtcNow.AddDays(-5),
                    ResolvedDate = DateTime.UtcNow.AddDays(-4)
                }
            };

            context.Bugs.AddRange(bugs);
            await context.SaveChangesAsync();

            // Add a couple of sample comments once bug IDs exist
            var firstBug = bugs[1];
            context.Comments.Add(new Comment
            {
                BugId = firstBug.Id,
                UserId = developer.Id,
                Text = "Looking into this — seems like a caching issue on the dashboard query.",
                CreatedDate = DateTime.UtcNow.AddHours(-5)
            });

            await context.SaveChangesAsync();
        }
    }
}