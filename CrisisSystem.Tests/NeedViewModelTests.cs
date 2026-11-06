
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using _202Oppgave.ViewModels;
using Xunit;

namespace CrisisSystem.Tests;

public class NeedViewModelTests
{
    private static NeedViewModel CreateValidNeed()
    {
        return new NeedViewModel
        {
            Type = "Generator",
            Location = "Kristiansand",
            Priority = NeedPriority.Akutt,
            Description = "Trenger strømforsyning",
            Deadline = DateTime.Today.AddDays(2),
            Latitude = 58.1467,
            Longitude = 7.9956
        };
    }

    private static List<ValidationResult> Validate(NeedViewModel model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        Validator.TryValidateObject(
            model,
            context,
            results,
            validateAllProperties: true
        );

        return results;
    }

    [Fact]
    public void ValidNeed_ShouldPassValidation()
    {
        var model = CreateValidNeed();

        var results = Validate(model);

        Assert.Empty(results);
    }

    [Fact]
    public void MissingType_ShouldFailValidation()
    {
        var model = CreateValidNeed();
        model.Type = "";

        var results = Validate(model);

        Assert.Contains(results,
            r => r.MemberNames.Contains(nameof(model.Type)));
    }

    [Fact]
    public void MissingLocation_ShouldFailValidation()
    {
        var model = CreateValidNeed();
        model.Location = "";

        var results = Validate(model);

        Assert.Contains(results,
            r => r.MemberNames.Contains(nameof(model.Location)));
    }

    [Fact]
    public void MissingPriority_ShouldFailValidation()
    {
        var model = CreateValidNeed();
        model.Priority = null;

        var results = Validate(model);

        Assert.Contains(results,
            r => r.MemberNames.Contains(nameof(model.Priority)));
    }

    [Fact]
    public void MissingDescription_ShouldFailValidation()
    {
        var model = CreateValidNeed();
        model.Description = "";

        var results = Validate(model);

        Assert.Contains(results,
            r => r.MemberNames.Contains(nameof(model.Description)));
    }

    [Fact]
    public void MissingDeadline_ShouldFailValidation()
    {
        var model = CreateValidNeed();
        model.Deadline = null;

        var results = Validate(model);

        Assert.Contains(results,
            r => r.MemberNames.Contains(nameof(model.Deadline)));
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void InvalidLatitude_ShouldFailValidation(double latitude)
    {
        var model = CreateValidNeed();
        model.Latitude = latitude;

        var results = Validate(model);

        Assert.Contains(results,
            r => r.MemberNames.Contains(nameof(model.Latitude)));
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void InvalidLongitude_ShouldFailValidation(double longitude)
    {
        var model = CreateValidNeed();
        model.Longitude = longitude;

        var results = Validate(model);

        Assert.Contains(results,
            r => r.MemberNames.Contains(nameof(model.Longitude)));
    }
}
