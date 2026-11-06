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
}