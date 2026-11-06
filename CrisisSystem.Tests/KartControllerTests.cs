using _202Oppgave.Controllers;
using _202Oppgave.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CrisisSystem.Tests;

public class KartControllerTests
{
    [Fact]
    public void MissingLatitude_ShouldReturnViewWithError()
    {
        var controller = new KartController();
        var model = new RegistrerPunktViewModel
        {
            Latitude = null,
            Longitude = 7.9956
        };

        var result = controller.Index(model);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public void MissingLongitude_ShouldReturnViewWithError()
    {
        var controller = new KartController();
        var model = new RegistrerPunktViewModel
        {
            Latitude = 58.1467,
            Longitude = null
        };

        var result = controller.Index(model);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }
    
    
    [Fact]
    public void ValidMapPoint_ShouldRedirectToOverview()
    {
        var controller = new KartController();
        var model = new RegistrerPunktViewModel
        {
            Latitude = 58.1467,
            Longitude = 7.9956,
            Kategori = "behov",
            Type = "Drikkevann",
            Beskrivelse = "Behov for drikkevann"
        };

        var result = controller.Index(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Oversikt", redirect.ActionName);
    }

    [Fact]
    public void ValidMapPoint_ShouldNotProduceValidationError()
    {
        var controller = new KartController();
        var model = new RegistrerPunktViewModel
        {
            Latitude = 58.1467,
            Longitude = 7.9956,
            Kategori = "ressurs",
            Type = "Generator",
            Beskrivelse = "Tilgjengelig generator"
        };

        controller.Index(model);

        Assert.True(controller.ModelState.IsValid);
    }

}