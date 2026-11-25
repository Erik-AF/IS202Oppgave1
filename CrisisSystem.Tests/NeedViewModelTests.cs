using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;
using _202Oppgave.ViewModels;

namespace CrisisSystem.Tests;

public class NeedViewModelTests
{
    [Fact]
    public void NeedWithoutCoordinates_ShouldFailValidation()
    {

        var model = new NeedViewModel
        {
            Type = "Mat",
            Location = "Kristiansand",
            Priority = NeedPriority.Akutt,
            Description = "Trenger matforsyninger",
            Deadline = DateTime.Now.AddDays(1),
            Latitude = null,
            Longitude = null
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(
            model, context, results, validateAllProperties: true);

        Assert.False(isValid);

        Assert.Contains(results, result =>
            result.ErrorMessage == "Velg et punkt på kartet.");
    }

    [Fact]
    public void NeedWithInvalidLatitude_ShouldFailValidation()
    {

        var model = new NeedViewModel
        {
            Type = "Vann",
            Location = "Kristiansand",
            Priority = NeedPriority.Akutt,
            Description = "Trenger vann",
            Deadline = DateTime.Now.AddDays(1),
            Latitude = 100,
            Longitude = 8
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(
            model, context, results, validateAllProperties: true);

        Assert.False(isValid);

        Assert.Contains(results, result =>
            result.ErrorMessage == "Ugyldig breddegrad.");
    }

}
