using System.Security.Claims;
using _202Oppgave.Controllers;
using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models.Authorization;
using _202Oppgave.Models.Entities;
using _202Oppgave.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CrisisSystem.Tests;

public class ResourceProviderControllerTests
{
    private static (ResourceProviderController Controller, Mock<UserManager<ApplicationUser>> Users,
        Mock<SignInManager<ApplicationUser>> SignIn, Mock<IResourceRepository> Repository, ApplicationUser User)
        Setup(string role = RoleNames.ResourceProvider)
    {
        var user = new ApplicationUser
        {
            Id = "user-123", Email = "test@example.com", UserName = "test@example.com",
            FullName = "Test Person", PhoneNumber = "12345678"
        };
        var users = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<ApplicationUser>>>());

        var signIn = new Mock<SignInManager<ApplicationUser>>(
            users.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<ApplicationUser>>());

        var repository = new Mock<IResourceRepository>();
        users.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        users.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(user.Id);
        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, role)
            }, "Test"))
        };
        var controller = new ResourceProviderController(users.Object, signIn.Object, repository.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = http },
            TempData = new TempDataDictionary(http, Mock.Of<ITempDataProvider>())
        };
        return (controller, users, signIn, repository, user);
    }

    [Fact]
    public async Task Index_ShouldRequestOwnResourcesAndShowAtMostFour()
    {
        var (controller, _, _, repository, user) = Setup();
        var resources = Enumerable.Range(1, 6)
            .Select(id => new Resource { Id = id, UserId = user.Id }).ToList();
        repository.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(resources);

        var result = await controller.Index();

        var model = Assert.IsType<List<Resource>>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(new[] { 1, 2, 3, 4 }, model.Select(r => r.Id));
        repository.Verify(x => x.GetByUserIdAsync(user.Id), Times.Once);
        repository.Verify(x => x.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task Profile_ValidModel_ShouldSaveAndRefreshSignIn()
    {
        var (controller, users, signIn, _, user) = Setup();
        var model = new ProfileViewModel
        {
            FullName = " New Name ", PhoneNumber = " 87654321 ", Email = " new@example.com "
        };
        users.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        signIn.Setup(x => x.RefreshSignInAsync(user)).Returns(Task.CompletedTask);

        var result = await controller.Profile(model);

        Assert.Equal("Profile", Assert.IsType<RedirectToActionResult>(result).ActionName);
        users.Verify(x => x.UpdateAsync(It.Is<ApplicationUser>(u => u.Id == user.Id &&
            u.FullName == "New Name" && u.PhoneNumber == "87654321" &&
            u.Email == "new@example.com" && u.UserName == u.Email)), Times.Once);
        signIn.Verify(x => x.RefreshSignInAsync(user), Times.Once);
    }

    [Fact]
    public async Task Profile_InvalidModel_ShouldReturnFormWithoutSaving()
    {
        var (controller, users, signIn, _, _) = Setup();
        var model = new ProfileViewModel();
        controller.ModelState.AddModelError("Email", "Ugyldig e-post");

        var result = await controller.Profile(model);

        Assert.Same(model, Assert.IsType<ViewResult>(result).Model);
        users.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        signIn.Verify(x => x.RefreshSignInAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task Profile_EmailAlreadyUsed_ShouldRejectWithoutSaving()
    {
        var (controller, users, signIn, _, user) = Setup();
        var model = new ProfileViewModel
        {
            FullName = "New Name", PhoneNumber = "87654321", Email = "taken@example.com"
        };
        users.Setup(x => x.FindByEmailAsync(model.Email))
            .ReturnsAsync(new ApplicationUser { Id = "other-user", Email = model.Email });

        var result = await controller.Profile(model);

        Assert.Same(model, Assert.IsType<ViewResult>(result).Model);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal("test@example.com", user.Email);
        users.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        signIn.Verify(x => x.RefreshSignInAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }
}
