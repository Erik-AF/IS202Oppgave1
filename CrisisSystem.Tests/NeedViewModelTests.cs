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
        // Arrange: Opprett et behov med gyldige verdier,
        // men uten koordinater.
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

        // Act: Valider modellen.
        bool isValid = Validator.TryValidateObject(
            model, context, results, validateAllProperties: true);

        // Assert: Modellen skal være ugyldig.
        Assert.False(isValid);

        Assert.Contains(results, result =>
            result.ErrorMessage == "Velg et punkt på kartet.");
    }



    [Fact]
    public void NeedWithInvalidLatitude_ShouldFailValidation()
    {
        // Arrange: Opprett et behov med ugyldig breddegrad.
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

        // Act: Valider modellen.
        bool isValid = Validator.TryValidateObject(
            model, context, results, validateAllProperties: true);

        // Assert: Modellen skal være ugyldig.
        Assert.False(isValid);

        Assert.Contains(results, result =>
            result.ErrorMessage == "Ugyldig breddegrad.");
    }

}
