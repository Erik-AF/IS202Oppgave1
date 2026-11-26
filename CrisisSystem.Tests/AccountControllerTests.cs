using _202Oppgave.Controllers;
using _202Oppgave.Models.Entities;
using _202Oppgave.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
using Xunit;
using System.Text;

namespace CrisisSystem.Tests;

public class AccountControllerTests
{
    private readonly IdentityTestContext identity = new();
    private readonly Mock<IEmailSender> email = new();
    private AccountController Controller() => identity.Attach(new AccountController(identity.Users.Object, identity.SignIn.Object, email.Object));
    private static RegisterViewModel Registration(string role = "ResourceProvider") => new() {
        Email = "new@example.test", Password = "Valid-Password123!", ConfirmPassword = "Valid-Password123!",
        FullName = " New User ", PhoneNumber = " 12345678 ", Role = role
    };

    [Theory]
    [InlineData("Admin")]
    [InlineData("Unknown")]
    public async Task Register_ForbiddenRole_DoesNotCreateAccount(string role)
    {
        var controller = Controller(); var model = Registration(role);
        Assert.Same(model, Assert.IsType<ViewResult>(await controller.Register(model)).Model);
        Assert.False(controller.ModelState.IsValid);
        identity.Users.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Register_InvalidModel_PreservesFormWithoutCreatingAccount()
    {
        var controller = Controller(); var model = Registration();
        controller.ModelState.AddModelError("Email", "Invalid email");
        Assert.Same(model, Assert.IsType<ViewResult>(await controller.Register(model)).Model);
        identity.Users.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("ResourceProvider")]
    [InlineData("PublicActor")]
    public async Task Register_Valid_CreatesAllowedRoleAndRedirectsToLogin(string role)
    {
        identity.Users.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        identity.Users.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), role)).ReturnsAsync(IdentityResult.Success);
        Assert.Equal("Login", Assert.IsType<RedirectToActionResult>(await Controller().Register(Registration(role))).ActionName);
        identity.Users.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u => u.Email == "new@example.test" && u.UserName == u.Email && u.FullName == "New User" && u.PhoneNumber == "12345678"), "Valid-Password123!"), Times.Once);
        identity.Users.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), role), Times.Once);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ShowsErrorWithoutAssigningRole()
    {
        identity.Users.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "Email exists" }));
        var controller = Controller();
        Assert.IsType<ViewResult>(await controller.Register(Registration()));
        Assert.Contains(controller.ModelState.Values.SelectMany(v => v.Errors), e => e.ErrorMessage == "Email exists");
        identity.Users.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Register_RoleAssignmentFails_DeletesPartialAccount()
    {
        identity.Users.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        identity.Users.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role failed" }));
        identity.Users.Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
        Assert.IsType<ViewResult>(await Controller().Register(Registration()));
        identity.Users.Verify(x => x.DeleteAsync(It.Is<ApplicationUser>(u => u.Email == "new@example.test")), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Login_FailedOrLockedOut_ShowsError(bool lockedOut)
    {
        identity.Users.Setup(x => x.FindByEmailAsync(identity.User.Email!)).ReturnsAsync(identity.User);
        identity.SignIn.Setup(x => x.PasswordSignInAsync(identity.User, "wrong", false, true))
            .ReturnsAsync(lockedOut ? Microsoft.AspNetCore.Identity.SignInResult.LockedOut : Microsoft.AspNetCore.Identity.SignInResult.Failed);
        var controller = Controller(); var model = new LoginViewModel { Email = identity.User.Email!, Password = "wrong" };
        Assert.Same(model, Assert.IsType<ViewResult>(await controller.Login(model)).Model);
        Assert.False(controller.ModelState.IsValid);
    }

    [Theory]
    [InlineData("/ResourceProvider", true)]
    [InlineData("https://external.example", false)]
    public async Task Login_Success_OnlyUsesLocalReturnUrl(string returnUrl, bool local)
    {
        identity.Users.Setup(x => x.FindByEmailAsync(identity.User.Email!)).ReturnsAsync(identity.User);
        identity.SignIn.Setup(x => x.PasswordSignInAsync(identity.User, "correct", true, true)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        var controller = Controller(); var url = new Mock<IUrlHelper>();
        url.Setup(x => x.IsLocalUrl(returnUrl)).Returns(local); controller.Url = url.Object;
        var result = await controller.Login(new LoginViewModel { Email = identity.User.Email!, Password = "correct", RememberMe = true, ReturnUrl = returnUrl });
        if (local) Assert.Equal(returnUrl, Assert.IsType<RedirectResult>(result).Url);
        else { var redirect = Assert.IsType<RedirectToActionResult>(result); Assert.Equal("Home", redirect.ControllerName); Assert.Equal("Index", redirect.ActionName); }
    }

    [Fact]
    public async Task Login_InvalidModel_DoesNotAttemptSignIn()
    {
        var controller = Controller(); controller.ModelState.AddModelError("Email", "Required");
        Assert.IsType<ViewResult>(await controller.Login(new LoginViewModel()));
        identity.SignIn.Verify(x => x.PasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Logout_SignsOutAndRedirectsToLogin()
    {
        Assert.Equal("Login", Assert.IsType<RedirectToActionResult>(await Controller().Logout()).ActionName);
        identity.SignIn.Verify(x => x.SignOutAsync(), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ForgotPassword_UnknownOrUnconfirmedAccount_DoesNotRevealAccount(bool exists)
    {
        identity.Users.Setup(x => x.FindByEmailAsync("test@example.test")).ReturnsAsync(exists ? identity.User : null);
        identity.Users.Setup(x => x.IsEmailConfirmedAsync(identity.User)).ReturnsAsync(false);
        Assert.Equal("ForgotPasswordConfirmation", Assert.IsType<RedirectToActionResult>(await Controller().ForgotPassword(new ForgotPasswordViewModel { Email = "test@example.test" })).ActionName);
        email.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_ConfirmedAccount_SendsEncodedResetLink()
    {
        identity.Users.Setup(x => x.FindByEmailAsync(identity.User.Email!)).ReturnsAsync(identity.User);
        identity.Users.Setup(x => x.IsEmailConfirmedAsync(identity.User)).ReturnsAsync(true);
        identity.Users.Setup(x => x.GeneratePasswordResetTokenAsync(identity.User)).ReturnsAsync("token+/=");
        var controller = Controller(); var url = new Mock<IUrlHelper>();
        var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("token+/="));
        url.Setup(x => x.Action(It.Is<UrlActionContext>(a => a.Action == "ResetPassword" && a.Values!.GetType().GetProperty("code")!.GetValue(a.Values)!.Equals(encoded))))
            .Returns("https://example.test/reset?code=" + encoded);
        controller.Url = url.Object;
        Assert.IsType<RedirectToActionResult>(await controller.ForgotPassword(new ForgotPasswordViewModel { Email = identity.User.Email! }));
        email.Verify(x => x.SendEmailAsync(identity.User.Email!, "Reset your password", It.Is<string>(s => s.Contains(encoded))), Times.Once);
    }

    [Theory]
    [InlineData("!invalid", false)]
    [InlineData("dG9rZW4", true)]
    public async Task ResetPassword_MalformedOrRejectedToken_DoesNotSucceed(string token, bool reachesIdentity)
    {
        identity.Users.Setup(x => x.FindByEmailAsync(identity.User.Email!)).ReturnsAsync(identity.User);
        identity.Users.Setup(x => x.ResetPasswordAsync(identity.User, "token", "new-password")).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid or expired token" }));
        var controller = Controller();
        Assert.IsType<ViewResult>(await controller.ResetPassword(new ResetPasswordViewModel { Email = identity.User.Email!, Code = token, Password = "new-password" }));
        Assert.False(controller.ModelState.IsValid);
        identity.Users.Verify(x => x.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), reachesIdentity ? Times.Once() : Times.Never());
    }
}
