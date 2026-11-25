using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using _202Oppgave.ViewModels.Account;
using Xunit;

namespace CrisisSystem.Tests;

public class LoginViewModelTests
{
    private static List<ValidationResult> Validate(LoginViewModel model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        Validator.TryValidateObject(model, context, results, true);

        return results;
    }

    [Fact]
    public void ValidLogin_ShouldPassValidation()
    {
        var model = new LoginViewModel
        {
            Email = "bruker@example.com",
            Password = "Test123!"
        };

        var results = Validate(model);

        Assert.Empty(results);
    }

    [Fact]
    public void MissingEmail_ShouldFailValidation()
    {
        var model = new LoginViewModel
        {
            Email = "",
            Password = "Test123!"
        };

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(LoginViewModel.Email)));
    }

    [Fact]
    public void InvalidEmail_ShouldFailValidation()
    {
        var model = new LoginViewModel
        {
            Email = "ugyldig-epost",
            Password = "Test123!"
        };

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(LoginViewModel.Email)));
    }

    [Fact]
    public void MissingPassword_ShouldFailValidation()
    {
        var model = new LoginViewModel
        {
            Email = "bruker@example.com",
            Password = ""
        };

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(LoginViewModel.Password)));
    }
}