using EyeMezzexz.Data;
using EyeMezzexz.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace E_Commerce_Mezzex.Data
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var logger = serviceProvider.GetRequiredService<ILogger<SeedData>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                logger.LogInformation("Starting data seeding...");

                // Seed roles
                await SeedRolesAsync(roleManager, logger);

                // Seed permissions
                await SeedPermissionsAsync(context, logger);

                // Seed users and assign roles and permissions
                await SeedUsersAsync(userManager, roleManager, logger, context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
        {
            string[] roleNames = { "Registered", "Admin", "Administrator" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                    if (result.Succeeded)
                    {
                        logger.LogInformation($"Role '{roleName}' created successfully.");
                    }
                    else
                    {
                        logger.LogError($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private static async Task SeedPermissionsAsync(ApplicationDbContext context, ILogger logger)
        {
            if (!context.PermissionsName.Any())
            {
                var permissions = new[]
                {
                    new PermissionName { Name = "CreateCategory" },
                    new PermissionName { Name = "CreateBrand" },
                    new PermissionName { Name = "CreateProduct" },
                    new PermissionName { Name = "ManageSettings" }
                };

                await context.PermissionsName.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
                logger.LogInformation("Permissions seeded successfully.");
            }
        }

        private static async Task SeedUsersAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger logger,
            ApplicationDbContext context)
        {
            await CreateUserAsync(
                userManager, roleManager, logger,
                "islam@direct-pharmacy.co.uk", "Sonaislam@143#",
                "Super", "Admin", "Male",new[] { "Administrator" });

            await CreateUserAsync(
                userManager, roleManager, logger,
                "regularuser@example.com", "RegularUser@123",
                "Regular", "User", "Female", new[] { "Registered" });

            await AssignAllPermissionsToAdministrator(userManager, context, logger);
        }

        private static async Task CreateUserAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger logger,
            string email,
            string password,
            string firstName,
            string lastName,
            string gender,
            string[] roles)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Gender = gender,
                    Active = true
                };

                var createUserResult = await userManager.CreateAsync(user, password);
                if (createUserResult.Succeeded)
                {
                    logger.LogInformation($"User '{email}' created successfully.");

                    foreach (var role in roles)
                    {
                        if (await roleManager.RoleExistsAsync(role))
                        {
                            await userManager.AddToRoleAsync(user, role);
                            logger.LogInformation($"Role '{role}' assigned to user '{email}'.");
                        }
                        else
                        {
                            logger.LogWarning($"Role '{role}' does not exist.");
                        }
                    }

                    if (roles.Contains("Administrator"))
                    {
                        await AssignClaimsToUser(userManager, user, new[] { "CreateCategory", "CreateBrand", "CreateProduct", "ManageSettings" });
                    }
                }
                else
                {
                    logger.LogError($"Failed to create user '{email}': {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogInformation($"User '{email}' already exists.");
            }
        }

        private static async Task AssignClaimsToUser(UserManager<ApplicationUser> userManager, ApplicationUser user, string[] permissions)
        {
            foreach (var permission in permissions)
            {
                if (!(await userManager.GetClaimsAsync(user)).Any(c => c.Type == "Permission" && c.Value == permission))
                {
                    var claimResult = await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Permission", permission));
                    if (claimResult.Succeeded)
                    {
                        // Log success or continue silently
                    }
                    else
                    {
                        // Handle claim assignment failure if necessary
                    }
                }
            }
        }

        private static async Task AssignAllPermissionsToAdministrator(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger logger)
        {
            var administrator = await userManager.FindByEmailAsync("superadmin@example.com");
            if (administrator != null)
            {
                var permissions = context.PermissionsName.Select(p => p.Name).ToArray();
                await AssignClaimsToUser(userManager, administrator, permissions);
                logger.LogInformation("All permissions assigned to Administrator.");
            }
            else
            {
                logger.LogWarning("Administrator user not found.");
            }
        }
    }
}
