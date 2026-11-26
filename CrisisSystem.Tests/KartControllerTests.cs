using _202Oppgave.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CrisisSystem.Tests;

public class KartControllerTests
{
    [Fact]
    public void Oversikt_ShouldReturnView()
    {
        var controller = new KartController();

        var result = controller.Oversikt();

        Assert.IsType<ViewResult>(result);
    }

    [Theory]
    [InlineData(null, 7.9956)]
    [InlineData(58.1467, null)]
    [InlineData(91.0, 7.9956)]
    [InlineData(-91.0, 7.9956)]
    [InlineData(58.1467, 181.0)]
    [InlineData(58.1467, -181.0)]
    public async Task ReverseGeocode_InvalidCoordinates_ShouldReturnBadRequest(
        double? latitude, double? longitude)
    {
        var controller = new KartController();

        var result = await controller.ReverseGeocode(
            latitude, longitude);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}