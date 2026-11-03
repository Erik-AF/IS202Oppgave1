using _202Oppgave.Models.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace _202Oppgave.Services
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(
            RoleManager<IdentityRole> roleManager)
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
    }
}