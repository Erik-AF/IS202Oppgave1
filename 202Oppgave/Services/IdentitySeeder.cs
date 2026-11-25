
using _202Oppgave.Models.Entities;
using _202Oppgave.Models.Authorization;
using Microsoft.AspNetCore.Identity;

namespace _202Oppgave.Services
{
    /// Oppretter manglende roller og demonstrasjonsbrukere ved oppstart.
    public static class IdentitySeeder
    {
        /// Oppretter de faste rollene hvis de ikke finnes fra før.
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

        /// Sikrer at demonstrasjonsbrukeren for administrator finnes med riktig rolle.
        public static async Task SeedDevelopmentAdminAsync(
            UserManager<ApplicationUser> userManager,
            string password)
        {
            await SeedUserAsync(
                userManager,
                "admin@example.com",
                password,
                RoleNames.Admin);
        }

        /// Sikrer at demonstrasjonsbrukeren for ressursleverandør finnes med riktig rolle.
        public static async Task SeedDevelopmentResourceProviderAsync(
            UserManager<ApplicationUser> userManager,
            string password)
        {
            await SeedUserAsync(
                userManager,
                "provider@example.com",
                password,
                RoleNames.ResourceProvider);
        }

        /// Sikrer at demonstrasjonsbrukeren for offentlig aktør finnes med riktig rolle.
        public static async Task SeedDevelopmentPublicActorAsync(
            UserManager<ApplicationUser> userManager,
            string password)
        {
            await SeedUserAsync(
                userManager,
                "publicactor@example.com",
                password,
                RoleNames.PublicActor);
        }

        /// Oppretter en manglende demonstrasjonsbruker og sikrer rolletilknytningen.
        private static async Task SeedUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
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
