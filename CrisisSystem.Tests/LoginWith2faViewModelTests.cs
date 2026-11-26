using System.ComponentModel.DataAnnotations;
using _202Oppgave.ViewModels.Account;
using Xunit;

namespace CrisisSystem.Tests;

public class LoginWith2faViewModelTests
{
    [Theory]
    [InlineData("123456")]
    [InlineData("000001")]
    [InlineData("999999")]
    public void ValidCode_PassesValidation(string code)
    {
        var model = new LoginWith2faViewModel
        {
            Code = code
        };

        var context = new ValidationContext(model);

        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            model,
            context,
            results,
            validateAllProperties: true);

        Assert.True(isValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("abcdef")]
    [InlineData("12 345")]
    public void InvalidCode_FailsValidation(string code)
    {
        var model = new LoginWith2faViewModel
        {
            Code = code
        };

        var context = new ValidationContext(model);

        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            model,
            context,
            results,
            validateAllProperties: true);

        Assert.False(isValid);
    }
}