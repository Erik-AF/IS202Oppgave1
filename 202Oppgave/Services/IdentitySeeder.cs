using _202Oppgave.Models.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace _202Oppgave.Services
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles =
            {
            RoleNames.ResourceProvider,
            RoleNames.PublicActor,
            RoleNames.Admin
            };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(
                        new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Kunne ikke opprette rollen {role}: " +
                            string.Join(", ",
                                result.Errors.Select(e => e.Description)));
                    }
                }
            }
        }

        public static async Task SeedDevelopmentAdminAsync(UserManager<IdentityUser> userManager, string password)
        {
            await SeedUserAsync(
                userManager,
                "admin@example.com",
                password,
                RoleNames.Admin);
        }

        public static async Task SeedDevelopmentResourceProviderAsync(UserManager<IdentityUser> userManager, string password)
        {
            await SeedUserAsync(
                userManager,
                "provider@example.com",
                password,
                RoleNames.ResourceProvider);
        }

        public static async Task SeedDevelopmentPublicActorAsync(UserManager<IdentityUser> userManager, string password)
        {
            await SeedUserAsync(
                userManager,
                "publicactor@example.com",
                password,
                RoleNames.PublicActor);
        }

        private static async Task SeedUserAsync(UserManager<IdentityUser> userManager, string email, string password, string role)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    user, password);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                var result = await userManager.AddToRoleAsync(
                    user, role);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}