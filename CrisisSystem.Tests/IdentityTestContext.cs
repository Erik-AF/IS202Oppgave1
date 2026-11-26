using System.Security.Claims;
using _202Oppgave.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CrisisSystem.Tests;

internal sealed class IdentityTestContext
{
    public Mock<UserManager<ApplicationUser>> Users { get; }
    public Mock<SignInManager<ApplicationUser>> SignIn { get; }
    public ApplicationUser User { get; } = new() { Id = "owner", UserName = "owner@example.test", Email = "owner@example.test", FullName = "Test User", PhoneNumber = "12345678" };

    public IdentityTestContext()
    {
        Users = new(Mock.Of<IUserStore<ApplicationUser>>(), Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(), Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(), new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(), Mock.Of<IServiceProvider>(), Mock.Of<ILogger<UserManager<ApplicationUser>>>());
        SignIn = new(Users.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Options.Create(new IdentityOptions()), Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(), Mock.Of<IUserConfirmation<ApplicationUser>>());
        Users.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(User);
        Users.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(User.Id);
        Users.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
        SignIn.Setup(x => x.RefreshSignInAsync(It.IsAny<ApplicationUser>())).Returns(Task.CompletedTask);
        SignIn.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);
    }

    public T Attach<T>(T controller, string role = "ResourceProvider") where T : Controller
    {
        controller.ControllerContext = new() { HttpContext = new DefaultHttpContext {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, User.Id), new Claim(ClaimTypes.Role, role)
            }, "Test"))
        }};
        controller.TempData = new TempDataDictionary(controller.HttpContext, Mock.Of<ITempDataProvider>());
        return controller;
    }
}
