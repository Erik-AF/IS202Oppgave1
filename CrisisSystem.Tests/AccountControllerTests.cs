using _202Oppgave.Controllers;
using _202Oppgave.Models.Entities;
using _202Oppgave.Models.Authorization;
using _202Oppgave.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CrisisSystem.Tests;

public class AccountControllerTests
{
    private static (AccountController Controller, Mock<UserManager<ApplicationUser>> Users,
        Mock<SignInManager<ApplicationUser>> SignIn, Mock<IEmailSender> Email) Setup()
    {
        var users = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(), Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(), new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(), Mock.Of<IServiceProvider>(), Mock.Of<ILogger<UserManager<ApplicationUser>>>());
        var signIn = new Mock<SignInManager<ApplicationUser>>(
            users.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Options.Create(new IdentityOptions()), Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(), Mock.Of<IUserConfirmation<ApplicationUser>>());
        var email = new Mock<IEmailSender>();
        var controller = new AccountController(users.Object, signIn.Object, email.Object);
        return (controller, users, signIn, email);
    }

    private static RegisterViewModel ValidRegistration(string role = RoleNames.ResourceProvider)
    {
        return new RegisterViewModel
        {
            FullName = "Test Person",
            PhoneNumber = "12345678",
            Email = "test@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            Role = role
        };
    }

    [Fact]
    public async Task Register_AsAdmin_ShouldRejectWithoutCreatingUser()
    {
        var (controller, users, _, _) = Setup();
        var model = ValidRegistration(RoleNames.Admin);

        var result = await controller.Register(model);

        Assert.Same(model, Assert.IsType<ViewResult>(result).Model);
        Assert.True(controller.ModelState.ContainsKey(nameof(model.Role)));
        Assert.False(controller.ModelState.IsValid);
        users.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        users.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Register_InvalidModel_ShouldReturnFormWithoutCreatingUser()
    {
        var (controller, users, _, _) = Setup();
        var model = ValidRegistration();
        controller.ModelState.AddModelError("Email", "Ugyldig e-post");

        var result = await controller.Register(model);

        Assert.Same(model, Assert.IsType<ViewResult>(result).Model);
        users.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(RoleNames.ResourceProvider)]
    [InlineData(RoleNames.PublicActor)]
    public async Task Register_ValidModel_ShouldCreateUserAndRedirectToLogin(string role)
    {
        var (controller, users, _, _) = Setup();
        var model = ValidRegistration(role);
        users.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), model.Password)).ReturnsAsync(IdentityResult.Success);
        users.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), role)).ReturnsAsync(IdentityResult.Success);

        var result = await controller.Register(model);

        Assert.Equal("Login", Assert.IsType<RedirectToActionResult>(result).ActionName);
        users.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u =>
            u.Email == model.Email && u.UserName == model.Email &&
            u.FullName == model.FullName && u.PhoneNumber == model.PhoneNumber), model.Password), Times.Once);
        users.Verify(x => x.AddToRoleAsync(It.Is<ApplicationUser>(u => u.Email == model.Email), role), Times.Once);
    }

    [Fact]
    public void Register_Get_ShouldReturnEmptyForm()
    {
        var (controller, _, _, _) = Setup();

        var result = controller.Register();

        var model = Assert.IsType<RegisterViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Empty(model.Email);
        Assert.Empty(model.Password);
        Assert.Empty(model.FullName);
        Assert.Empty(model.Role);
    }

    [Fact]
    public void Login_Get_ShouldPreserveReturnUrl()
    {
        var (controller, _, _, _) = Setup();

        var result = controller.Login("/ResourceProvider");

        var model = Assert.IsType<LoginViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal("/ResourceProvider", model.ReturnUrl);
    }

    [Fact]
    public async Task Login_InvalidModel_ShouldReturnFormWithoutSigningIn()
    {
        var (controller, users, signIn, _) = Setup();
        var model = new LoginViewModel();
        controller.ModelState.AddModelError("Email", "E-post mangler");

        var result = await controller.Login(model);

        Assert.Same(model, Assert.IsType<ViewResult>(result).Model);
        users.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
        signIn.Verify(x => x.PasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_InvalidModel_ShouldReturnFormWithoutSendingEmail()
    {
        var (controller, users, _, email) = Setup();
        var model = new ForgotPasswordViewModel();
        controller.ModelState.AddModelError("Email", "E-post mangler");

        var result = await controller.ForgotPassword(model);

        Assert.Same(model, Assert.IsType<ViewResult>(result).Model);
        users.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
        email.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(null, "test@example.com")]
    [InlineData("code", null)]
    public void ResetPassword_MissingCodeOrEmail_ShouldReturnBadRequest(string? code, string? email)
    {
        var (controller, _, _, _) = Setup();

        var result = controller.ResetPassword(code, email);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_ValidCredentials_ShouldRedirectToLocalReturnUrl()
    {
        var (controller, users, signIn, _) = Setup();
        var user = new ApplicationUser { Email = "test@example.com" };
        var model = new LoginViewModel { Email = user.Email, Password = "Test123!", ReturnUrl = "/ResourceProvider" };
        users.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync(user);
        signIn.Setup(x => x.PasswordSignInAsync(user, model.Password, false, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        var url = new Mock<IUrlHelper>();
        url.Setup(x => x.IsLocalUrl(model.ReturnUrl)).Returns(true);
        controller.Url = url.Object;

        var result = await controller.Login(model);

        Assert.Equal(model.ReturnUrl, Assert.IsType<RedirectResult>(result).Url);
        signIn.Verify(x => x.PasswordSignInAsync(user, model.Password, false, true), Times.Once);
    }

    [Fact]
    public async Task Login_WrongPassword_ShouldReturnFormWithError()
    {
        var (controller, users, signIn, _) = Setup();
        var user = new ApplicationUser { Email = "test@example.com" };
        var model = new LoginViewModel { Email = user.Email, Password = "Wrong" };
        users.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync(user);
        signIn.Setup(x => x.PasswordSignInAsync(user, model.Password, false, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var result = await controller.Login(model);

        Assert.Same(model, Assert.IsType<ViewResult>(result).Model);
        Assert.False(controller.ModelState.IsValid);
        signIn.Verify(x => x.PasswordSignInAsync(user, model.Password, false, true), Times.Once);
    }

    [Fact]
    public async Task Logout_ShouldSignOutAndRedirectToLogin()
    {
        var (controller, _, signIn, _) = Setup();
        signIn.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);

        var result = await controller.Logout();

        Assert.Equal("Login", Assert.IsType<RedirectToActionResult>(result).ActionName);
        signIn.Verify(x => x.SignOutAsync(), Times.Once);
    }
}
