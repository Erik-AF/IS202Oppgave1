using System.Security.Claims;
using _202Oppgave.Controllers;
using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models.Entities;
using _202Oppgave.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
namespace CrisisSystem.Tests;
/// <summary>Tester NeedOwnership med kontrollerte testdata uten å starte hele applikasjonen.</summary>
public class NeedOwnershipTests
{
    /// <summary>Brukere uten eierskap får ikke redigere eller slette behovet.</summary>
    [Theory]
    [InlineData("other", "owner", true)]
    [InlineData("owner", null, true)]
    [InlineData(null, null, true)]
    [InlineData("owner", "owner", false)]
    public async Task NonOwnerCannotEditOrDelete(string? userId, string? ownerId, bool authenticated)
    {
        var (controller, repo) = Setup(userId, ownerId, authenticated);
        Assert.IsType<ForbidResult>(await controller.Edit(7));
        Assert.IsType<ForbidResult>(await controller.Edit(7, new NeedViewModel()));
        Assert.IsType<ForbidResult>(await controller.Delete(7));
        var view = Assert.IsType<ViewResult>(await controller.Confirmation(7));
        Assert.False(Assert.IsType<_202Oppgave.Models.Need>(view.Model).CanManage);
        repo.Verify(r => r.UpdateAsync(It.IsAny<Need>()), Times.Never);
        repo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
    /// <summary>Behovets eier får redigere og slette.</summary>
    [Fact]
    public async Task OwnerCanEditAndDelete()
    {
        var (controller, repo) = Setup("owner", "owner", true);
        Assert.IsType<ViewResult>(await controller.Edit(7));
        var view = Assert.IsType<ViewResult>(await controller.Confirmation(7));
        Assert.True(Assert.IsType<_202Oppgave.Models.Need>(view.Model).CanManage);
        Assert.IsType<RedirectToActionResult>(await controller.Edit(7, new NeedViewModel {
            Type = "Updated", Location = "Oslo", Description = "Help", Priority = NeedPriority.Akutt,
            Deadline = DateTime.Today.AddDays(1), Latitude = 60, Longitude = 10
        }));
        repo.Verify(r => r.UpdateAsync(It.Is<Need>(n => n.UserId == "owner" && n.Type == "Updated")), Times.Once);
        Assert.IsType<RedirectToActionResult>(await controller.Delete(7));
        repo.Verify(r => r.DeleteAsync(7), Times.Once);
    }
    /// <summary>Oppretter controller, innlogget testidentitet og repository-mocker for scenarioet.</summary>
    private static (NeedController, Mock<INeedRepository>) Setup(string? userId, string? ownerId, bool authenticated)
    {
        var repo = new Mock<INeedRepository>();
        repo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(new Need { Id = 7, UserId = ownerId });
        var claims = new List<Claim> { new(ClaimTypes.Role, "Admin") };
        if (userId != null) claims.Add(new(ClaimTypes.NameIdentifier, userId));
        var controller = new NeedController(repo.Object) { ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticated ? "test" : null)) }
        }};
        return (controller, repo);
    }
}
