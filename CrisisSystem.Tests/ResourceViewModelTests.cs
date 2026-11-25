using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.AspNetCore.Http;
using _202Oppgave.ViewModels;
using Xunit;

namespace CrisisSystem.Tests;

/// <summary>Tester ResourceViewModel med kontrollerte testdata uten å starte hele applikasjonen.</summary>
public class ResourceViewModelTests
{
    /// <summary>Oppretter gyldige grunnverdier slik at hver test kan variere ett vilkår.</summary>
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

    /// <summary>Validerer alle modellens attributter og eventuelle regler på tvers av felter.</summary>
    private static List<ValidationResult> Validate(ResourceViewModel model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        Validator.TryValidateObject(model, context, results, true);

        return results;
    }

    /// <summary>En gyldig ressursmodell godtas.</summary>
    [Fact]
    public void ValidResource_ShouldPassValidation()
    {
        var model = CreateValidResource();

        var results = Validate(model);

        Assert.Empty(results);
    }

    /// <summary>Manglende ressurstype avvises.</summary>
    [Fact]
    public void MissingType_ShouldFailValidation()
    {
        var model = CreateValidResource();
        model.Type = "";

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.Type)));
    }

    /// <summary>Manglende sted avvises.</summary>
    [Fact]
    public void MissingArea_ShouldFailValidation()
    {
        var model = CreateValidResource();
        model.Area = "";

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.Area)));
    }

    /// <summary>Tomt kontaktpersonfelt godtas i ressursmodellen.</summary>
    [Fact]
    public void EmptyContactName_ShouldPassValidation()
    {
        var model = CreateValidResource();
        model.ContactName = "";

        var results = Validate(model);

        Assert.DoesNotContain(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.ContactName)));
    }

    /// <summary>Tomt telefonfelt godtas i ressursmodellen.</summary>
    [Fact]
    public void EmptyContactPhone_ShouldPassValidation()
    {
        var model = CreateValidResource();
        model.ContactPhone = "";

        var results = Validate(model);

        Assert.DoesNotContain(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.ContactPhone)));
    }

    /// <summary>Manglende kategori avvises.</summary>
    [Fact]
    public void MissingCategory_ShouldFailValidation()
    {
        var model = CreateValidResource();
        model.CategoryId = null;

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.CategoryId)));
    }

    /// <summary>Ugyldig breddegrad avvises.</summary>
    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void InvalidLatitude_ShouldFailValidation(double latitude)
    {
        var model = CreateValidResource();
        model.Latitude = latitude;

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.Latitude)));
    }

    /// <summary>Ugyldig lengdegrad avvises.</summary>
    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void InvalidLongitude_ShouldFailValidation(double longitude)
    {
        var model = CreateValidResource();
        model.Longitude = longitude;

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.Longitude)));
    }

    /// <summary>Manglende breddegrad avvises.</summary>
    [Fact]
    public void MissingLatitude_ShouldFailValidation()
    {
        var model = CreateValidResource();
        model.Latitude = null;

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.Latitude)));
    }

    /// <summary>Manglende lengdegrad avvises.</summary>
    [Fact]
    public void MissingLongitude_ShouldFailValidation()
    {
        var model = CreateValidResource();
        model.Longitude = null;

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.Longitude)));
    }

    /// <summary>Ugyldig e-postformat avvises.</summary>
    [Fact]
    public void InvalidEmail_ShouldFailValidation()
    {
        var model = CreateValidResource();
        model.ContactEmail = "ugyldig-epost";

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.ContactEmail)));
    }

    /// <summary>Gyldig e-postformat godtas.</summary>
    [Fact]
    public void ValidEmail_ShouldPassValidation()
    {
        var model = CreateValidResource();
        model.ContactEmail = "kontakt@example.com";

        var results = Validate(model);

        Assert.Empty(results);
    }

    /// <summary>Sluttdato før startdato avvises.</summary>
    [Fact]
    public void AvailableToBeforeAvailableFrom_ShouldFailValidation()
    {
        var model = CreateValidResource();
        model.AvailableFrom = new DateTime(2026, 11, 10);
        model.AvailableTo = new DateTime(2026, 11, 9);

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.AvailableTo)));
    }

    /// <summary>En gyldig tilgjengelighetsperiode godtas.</summary>
    [Fact]
    public void ValidAvailabilityPeriod_ShouldPassValidation()
    {
        var model = CreateValidResource();
        model.AvailableFrom = new DateTime(2026, 11, 9);
        model.AvailableTo = new DateTime(2026, 11, 10);

        var results = Validate(model);

        Assert.Empty(results);
    }

    /// <summary>Bildefiler med filtype utenfor de tillatte formatene avvises.</summary>
    [Theory]
    [InlineData("bilde.pdf")]
    [InlineData("bilde.gif")]
    [InlineData("bilde.txt")]
    public void InvalidImageType_ShouldFailValidation(string fileName)
    {
        var model = CreateValidResource();
        model.Image = CreateImage(fileName, 1024);

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.Image)));
    }

    /// <summary>Bildefiler med tillatt filtype godtas.</summary>
    [Theory]
    [InlineData("bilde.jpg")]
    [InlineData("bilde.jpeg")]
    [InlineData("bilde.png")]
    [InlineData("bilde.webp")]
    public void ValidImageType_ShouldPassValidation(string fileName)
    {
        var model = CreateValidResource();
        model.Image = CreateImage(fileName, 1024);

        var results = Validate(model);

        Assert.Empty(results);
    }

    /// <summary>Bildefiler over fem megabyte avvises.</summary>
    [Fact]
    public void ImageLargerThan5MB_ShouldFailValidation()
    {
        var model = CreateValidResource();
        model.Image = CreateImage("bilde.jpg", ResourceViewModel.MaxImageBytes + 1);

        var results = Validate(model);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(ResourceViewModel.Image)));
    }

    /// <summary>En bildefil på nøyaktig fem megabyte godtas.</summary>
    [Fact]
    public void ImageExactly5MB_ShouldPassValidation()
    {
        var model = CreateValidResource();
        model.Image = CreateImage("bilde.jpg", ResourceViewModel.MaxImageBytes);

        var results = Validate(model);

        Assert.Empty(results);
    }

    /// <summary>Oppretter en simulert bildefil med ønsket navn og størrelse.</summary>
    private static IFormFile CreateImage(string fileName, long size)
    {
        var stream = new MemoryStream();
        stream.SetLength(size);

        return new FormFile(stream, 0, size, "Image", fileName);
    }
}
