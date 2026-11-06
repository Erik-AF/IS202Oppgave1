using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using _202Oppgave.ViewModels;
using Xunit;

namespace CrisisSystem.Tests;

public class ResourceViewModelTests
{
    private static ResourceViewModel CreateValidResource()
    {
        return new ResourceViewModel
        {
            Type = "Ambulanse",
            Area = "Kristiansand",
            Latitude = 58.1467,
            Longitude = 7.9956,
            ContactName = "Ola Nordmann",
            ContactPhone = "12345678",
            CategoryId = 1
        };
    }

    private static List<ValidationResult> Validate(ResourceViewModel model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        Validator.TryValidateObject(model, context, results, true);

        return results;
    }

    [Fact]
    public void ValidResource_ShouldPassValidation()
    {
        var model = CreateValidResource();

        var results = Validate(model);

        Assert.Empty(results);
    }

    [Fact]
    public void MissingType_ShouldFailValidation()
    {
        var model = CreateValidResource();
        model.Type = "";

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.Type)));
    }

    [Fact]
    public void MissingArea_ShouldFailValidation()
    {
        var model = CreateValidResource();
        model.Area = "";

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.Area)));
    }

    [Fact]
    public void MissingContactName_ShouldFailValidation()
    {
        var model = CreateValidResource();
        model.ContactName = "";

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.ContactName)));
    }

    [Fact]
    public void MissingContactPhone_ShouldFailValidation()
    {
        var model = CreateValidResource();
        model.ContactPhone = "";

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.ContactPhone)));
    }

    [Fact]
    public void MissingCategory_ShouldFailValidation()
    {
        var model = CreateValidResource();
        model.CategoryId = null;

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.CategoryId)));
    }
}
