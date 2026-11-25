using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using _202Oppgave.ViewModels.Account;
using Xunit;

namespace CrisisSystem.Tests;

/// <summary>Tester LoginViewModel med kontrollerte testdata uten å starte hele applikasjonen.</summary>
public class LoginViewModelTests
{
    /// <summary>Validerer alle modellens attributter og eventuelle regler på tvers av felter.</summary>
    private static List<ValidationResult> Validate(LoginViewModel model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        Validator.TryValidateObject(model, context, results, true);

        return results;
    }

    /// <summary>Gyldige innloggingsfelt godtas.</summary>
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

    /// <summary>Manglende e-post avvises.</summary>
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

    /// <summary>Ugyldig e-postformat avvises.</summary>
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

    /// <summary>Manglende passord avvises.</summary>
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