using _202Oppgave.Controllers;
using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models.Entities;
using _202Oppgave.Services;
using _202Oppgave.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace CrisisSystem.Tests;

public class NearbyAllocationTests
{
    private static Resource At(int id, double km, string status = "Tilgjengelig") => new()
    {
        Id = id, Type = $"Ressurs {id}", Latitude = km / 6371 * 180 / Math.PI,
        Longitude = 0, Status = status
    };
    private static Need Need() => new() { Id = 7, Type = "Hjelp", Latitude = 0, Longitude = 0 };

    [Fact]
    public void Suggestions_ShouldReturnOnlyThreeNearestAvailableResources()
    {
        var resources = new[] { At(1,4), At(2,1), At(3,3), At(4,2), At(5,0.5,"Utilgjengelig"), At(6,6) };
        Assert.Equal(new[] { 2,4,3 }, NearbyResourceSuggestions.Find(Need(), resources).Select(r => r.Id));
    }

    [Fact]
    public void Suggestions_ShouldRespectFiveKilometreBoundaryAndAllowFewerThanThree()
    {
        var result = NearbyResourceSuggestions.Find(Need(), new[] { At(1,4.9999), At(2,5.0001), At(3,0) });
        Assert.Equal(new[] { 3,1 }, result.Select(r => r.Id));
        Assert.InRange(result[1].DistanceKm,4.9998,5.0);
    }

    [Fact]
    public void Suggestions_ShouldIgnoreMissingOrInvalidCoordinates()
    {
        var missing = At(1,0); missing.Latitude = null;
        var invalid = At(2,0); invalid.Longitude = double.NaN;
        var outside = At(3,0); outside.Latitude = 91;
        Assert.Empty(NearbyResourceSuggestions.Find(Need(), new[] { missing, invalid, outside }));
        var need = Need(); need.Latitude = double.NaN;
        Assert.Empty(NearbyResourceSuggestions.Find(need, new[] { At(4,0) }));
    }

    [Fact]
    public void Suggestions_ShouldCalculateLongitudeDistanceAtNorwegianLatitude()
    {
        var need = Need(); need.Latitude = 60;
        var resource = At(1,0); resource.Latitude = 60; resource.Longitude = 0.05;
        Assert.InRange(Assert.Single(NearbyResourceSuggestions.Find(need,new[] { resource })).DistanceKm, 2.77,2.79);
    }

    private static (AllocationController Controller, Mock<IAllocationRepository> Allocations) Setup(List<Resource> resources, bool exists = true)
    {
        var allocations = new Mock<IAllocationRepository>();
        allocations.Setup(a => a.AllocateAsync(7, It.IsAny<List<int>>(), It.IsAny<string>())).ReturnsAsync(1);
        var resourceRepo = new Mock<IResourceRepository>();
        resourceRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(resources);
        var needs = new Mock<INeedRepository>();
        needs.Setup(n => n.GetByIdAsync(7)).ReturnsAsync(exists ? Need() : null);
        var controller = new AllocationController(allocations.Object,resourceRepo.Object,needs.Object);
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(),Mock.Of<ITempDataProvider>());
        return (controller,allocations);
    }

    [Fact]
    public async Task Get_ShouldShowSuggestionsWithoutAllocating()
    {
        var (controller,allocations) = Setup(new() { At(1,1) });
        var view = Assert.IsType<ViewResult>(await controller.FromNeed(7));
        var model = Assert.IsType<NeedAllocationViewModel>(view.Model);
        Assert.Equal(7,model.NeedId);
        Assert.Single(model.Suggestions);
        allocations.Verify(a => a.AllocateAsync(It.IsAny<int>(),It.IsAny<List<int>>(),It.IsAny<string>()),Times.Never);
    }

    [Fact]
    public async Task MissingNeed_ShouldReturnNotFound()
    {
        var (controller,_) = Setup(new(),false);
        Assert.IsType<NotFoundResult>(await controller.FromNeed(7));
        Assert.IsType<NotFoundResult>(await controller.FromNeed(new NeedAllocationViewModel { NeedId = 7 }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(999)]
    public async Task Post_ShouldRejectEmptyUnavailableDistantOrUnknownSelections(int selected)
    {
        var (controller,allocations) = Setup(new() { At(1,1),At(2,1,"Utilgjengelig"),At(3,6) });
        var model = new NeedAllocationViewModel { NeedId = 7,ResourceIds = selected == 0 ? new() : new() { selected } };
        Assert.IsType<ViewResult>(await controller.FromNeed(model));
        Assert.False(controller.ModelState.IsValid);
        allocations.Verify(a => a.AllocateAsync(It.IsAny<int>(),It.IsAny<List<int>>(),It.IsAny<string>()),Times.Never);
    }

    [Fact]
    public async Task Post_ShouldAllocateSelectedResourceToOriginalNeedOnlyOnce()
    {
        var (controller,allocations) = Setup(new() { At(1,1),At(2,2) });
        var result = Assert.IsType<RedirectToActionResult>(await controller.FromNeed(new NeedAllocationViewModel
        { NeedId = 7, ResourceIds = new() { 1,1 }, Note = "Avtalt" }));
        allocations.Verify(a => a.AllocateAsync(7,It.Is<List<int>>(ids => ids.Count == 1 && ids[0] == 1),"Avtalt"),Times.Once);
        Assert.Equal("Confirmation",result.ActionName);
        Assert.Equal(7,result.RouteValues!["id"]);
    }

    [Fact]
    public async Task Post_InvalidNote_ShouldPreserveFormWithoutAllocating()
    {
        var (controller,allocations) = Setup(new() { At(1,1) });
        controller.ModelState.AddModelError("Note","Maks 500 tegn");
        var view = Assert.IsType<ViewResult>(await controller.FromNeed(new NeedAllocationViewModel { NeedId=7, ResourceIds=new() {1} }));
        Assert.Single(Assert.IsType<NeedAllocationViewModel>(view.Model).Suggestions);
        allocations.Verify(a => a.AllocateAsync(It.IsAny<int>(),It.IsAny<List<int>>(),It.IsAny<string>()),Times.Never);
    }
}
