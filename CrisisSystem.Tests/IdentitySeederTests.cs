using _202Oppgave.Services;
using _202Oppgave.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CrisisSystem.Tests;

public class IdentitySeederTests
{
    [Fact]
    public async Task SeedRoles_OnlyCreatesMissingRoles()
    {
        var manager = new Mock<RoleManager<IdentityRole>>(Mock.Of<IRoleStore<IdentityRole>>(),
            Array.Empty<IRoleValidator<IdentityRole>>(), new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(), Mock.Of<ILogger<RoleManager<IdentityRole>>>());
        manager.Setup(x => x.RoleExistsAsync("Admin")).ReturnsAsync(true);
        manager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);
        await IdentitySeeder.SeedRolesAsync(manager.Object);
        manager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Admin")), Times.Never);
        manager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "ResourceProvider")), Times.Once);
        manager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "PublicActor")), Times.Once);
    }

    [Fact]
    public async Task SeedExistingAdmin_DoesNotRecreateAccountOrChangePassword()
    {
        var identity = new IdentityTestContext();
        identity.Users.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(identity.User);
        identity.Users.Setup(x => x.IsInRoleAsync(identity.User, "Admin")).ReturnsAsync(true);
        await IdentitySeeder.SeedDevelopmentAdminAsync(identity.Users.Object, "Unused-Test123!");
        identity.Users.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        identity.Users.Verify(x => x.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        identity.Users.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }
}
