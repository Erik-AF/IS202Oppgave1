using _202Oppgave.Controllers;
using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models.Entities;
using _202Oppgave.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CrisisSystem.Tests;

public class MyPageTests
{
    [Fact]
    public async Task ProviderOverview_OnlyRequestsOwnResourcesAndShowsFour()
    {
        var identity = new IdentityTestContext(); var repo = new Mock<IResourceRepository>(MockBehavior.Strict);
        repo.Setup(x => x.GetByUserIdAsync(identity.User.Id)).ReturnsAsync(Enumerable.Range(1, 6).Select(id => new Resource { Id = id, UserId = identity.User.Id }).ToList());
        var controller = identity.Attach(new ResourceProviderController(identity.Users.Object, identity.SignIn.Object, repo.Object));
        var model = Assert.IsType<List<Resource>>(Assert.IsType<ViewResult>(await controller.Index()).Model);
        Assert.Equal(new[] { 1, 2, 3, 4 }, model.Select(r => r.Id));
        repo.Verify(x => x.GetByUserIdAsync(identity.User.Id), Times.Once);
    }

    [Theory]
    [InlineData("PublicActor")]
    [InlineData("Admin")]
    public async Task ActorOverview_OnlyShowsOwnNeeds(string role)
    {
        var identity = new IdentityTestContext(); var repo = new Mock<INeedRepository>();
        repo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Need> { new() { Id = 1, UserId = identity.User.Id }, new() { Id = 2, UserId = "other" }, new() { Id = 3 } });
        var controller = identity.Attach(new PublicActorController(identity.Users.Object, identity.SignIn.Object, repo.Object), role);
        foreach (var result in new[] { await controller.Index(), await controller.Needs() })
            Assert.Equal(1, Assert.Single(Assert.IsType<List<Need>>(Assert.IsType<ViewResult>(result).Model)).Id);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Profile_DuplicateEmail_DoesNotSaveOrRefreshLogin(bool actor)
    {
        var identity = new IdentityTestContext();
        identity.Users.Setup(x => x.FindByEmailAsync("taken@example.test")).ReturnsAsync(new ApplicationUser { Id = "other" });
        var controller = Page(identity, actor); var model = new ProfileViewModel { Email = "taken@example.test", FullName = "New", PhoneNumber = "12345678" };
        Assert.Same(model, Assert.IsType<ViewResult>(await Save(controller, model)).Model);
        Assert.False(controller.ModelState.IsValid);
        identity.Users.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        identity.SignIn.Verify(x => x.RefreshSignInAsync(It.IsAny<ApplicationUser>()), Times.Never);
        Assert.Equal("owner@example.test", identity.User.Email);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Profile_ValidChange_UpdatesEmailAndUsernameAndRefreshesLogin(bool actor)
    {
        var identity = new IdentityTestContext(); var controller = Page(identity, actor);
        Assert.Equal("Profile", Assert.IsType<RedirectToActionResult>(await Save(controller, new ProfileViewModel {
            FullName = " New Name ", Email = " new@example.test ", PhoneNumber = " 87654321 "
        })).ActionName);
        identity.Users.Verify(x => x.UpdateAsync(It.Is<ApplicationUser>(u => u.FullName == "New Name" && u.Email == "new@example.test" && u.UserName == u.Email && u.PhoneNumber == "87654321")), Times.Once);
        identity.SignIn.Verify(x => x.RefreshSignInAsync(identity.User), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Profile_InvalidModel_PreservesFormWithoutSaving(bool actor)
    {
        var identity = new IdentityTestContext(); var controller = Page(identity, actor); var model = new ProfileViewModel();
        controller.ModelState.AddModelError("Email", "Invalid");
        Assert.Same(model, Assert.IsType<ViewResult>(await Save(controller, model)).Model);
        identity.Users.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        identity.SignIn.Verify(x => x.RefreshSignInAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    private static Controller Page(IdentityTestContext identity, bool actor) => actor
        ? identity.Attach(new PublicActorController(identity.Users.Object, identity.SignIn.Object, Mock.Of<INeedRepository>()), "PublicActor")
        : identity.Attach(new ResourceProviderController(identity.Users.Object, identity.SignIn.Object, Mock.Of<IResourceRepository>()));
    private static Task<IActionResult> Save(Controller page, ProfileViewModel model) => page is PublicActorController actor ? actor.Profile(model) : ((ResourceProviderController)page).Profile(model);
}
