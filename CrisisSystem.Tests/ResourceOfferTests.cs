
using System.Security.Claims;
using _202Oppgave.Controllers;
using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models.Authorization;
using _202Oppgave.Models.Entities;
using _202Oppgave.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace CrisisSystem.Tests;

public class ResourceOfferTests
{
    private static (AllocationController Controller, Mock<IAllocationRepository> Repo) Setup(
        string role = RoleNames.ResourceProvider, string userId = "provider", string resourceOwner = "provider", string? needOwner = "actor")
    {
        var allocations = new Mock<IAllocationRepository>();
        allocations.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Allocation
        {
            AllocationId = 10, NeedId = 7, State = "Forespurt",
            Need = new Need { Id = 7, UserId = "actor" }
        });
        allocations.Setup(r => r.OfferAsync(7, It.IsAny<List<int>>(), "provider", It.IsAny<string>())).ReturnsAsync(1);
        allocations.Setup(r => r.ApproveAsync(10)).ReturnsAsync(true);
        allocations.Setup(r => r.RemoveAsync(10)).ReturnsAsync(7);
        var resources = new Mock<IResourceRepository>();
        resources.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Resource>
        {
            new() { Id = 1, UserId = resourceOwner, Type = "Aggregat", Status = "Tilgjengelig" }
        });
        resources.Setup(r => r.GetByUserIdAsync("provider")).ReturnsAsync(new List<Resource>
        {
            new() { Id = 1, UserId = "provider", Type = "Aggregat", Status = "Tilgjengelig" },
            new() { Id = 2, UserId = "provider", Type = "Bil", Status = "Reservert" }
        });
        var needs = new Mock<INeedRepository>();
        var need = new Need { Id = 7, UserId = needOwner, Type = "Strøm" };
        needs.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(need);
        needs.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Need> { need });
        var http = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Role, role)
        }, "Test")) };
        return (new AllocationController(allocations.Object, resources.Object, needs.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = http },
            TempData = new TempDataDictionary(http, Mock.Of<ITempDataProvider>())
        }, allocations);
    }

    [Fact]
    public async Task Provider_Get_ShouldShowOfferFormForOwnResource()
    {
        var (controller, _) = Setup();
        var view = Assert.IsType<ViewResult>(await controller.Create(new List<int> { 1 }));
        var model = Assert.IsType<AllocateViewModel>(view.Model);
        Assert.True(model.IsOffer);
        Assert.Single(model.Resources);
        Assert.Single(model.NeedOptions);
    }

    [Fact]
    public async Task Provider_CannotOfferSomeoneElsesResource()
    {
        var (controller, repo) = Setup(resourceOwner: "someone-else");
        Assert.IsType<ForbidResult>(await controller.Create(new List<int> { 1 }));
        Assert.IsType<ForbidResult>(await controller.Create(new AllocateViewModel { NeedId = 7, ResourceIds = new() { 1 } }));
        repo.Verify(r => r.OfferAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Provider_Post_ShouldSendOfferInsteadOfAllocating()
    {
        var (controller, repo) = Setup();
        Assert.IsType<RedirectToActionResult>(await controller.Create(new AllocateViewModel
        { NeedId = 7, ResourceIds = new() { 1 }, IsOffer = false }));
        repo.Verify(r => r.OfferAsync(7, It.IsAny<List<int>>(), "provider", null), Times.Once);
        repo.Verify(r => r.AllocateAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task InvalidForm_ShouldKeepOfferMode()
    {
        var (controller, repo) = Setup();
        controller.ModelState.AddModelError("NeedId", "Velg et behov");
        var view = Assert.IsType<ViewResult>(await controller.Create(new AllocateViewModel { ResourceIds = new() { 1 } }));
        Assert.True(Assert.IsType<AllocateViewModel>(view.Model).IsOffer);
        repo.Verify(r => r.OfferAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OtherActor_CannotApproveOrRemove()
    {
        var (controller, repo) = Setup(RoleNames.PublicActor, "other-actor");
        Assert.IsType<ForbidResult>(await controller.Approve(10, null));
        Assert.IsType<ForbidResult>(await controller.Remove(10, null));
        repo.Verify(r => r.ApproveAsync(It.IsAny<int>()), Times.Never);
        repo.Verify(r => r.RemoveAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Provider_CannotApproveEvenWithMatchingUserId()
    {
        var (controller, repo) = Setup(RoleNames.ResourceProvider, "actor");
        Assert.IsType<ForbidResult>(await controller.Approve(10, null));
        repo.Verify(r => r.ApproveAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task NeedOwner_CanApproveAndRemove()
    {
        var (controller, repo) = Setup(RoleNames.PublicActor, "actor");
        Assert.IsType<RedirectToActionResult>(await controller.Approve(10, null));
        Assert.IsType<RedirectToActionResult>(await controller.Remove(10, null));
        repo.Verify(r => r.ApproveAsync(10), Times.Once);
        repo.Verify(r => r.RemoveAsync(10), Times.Once);
    }

    [Fact]
    public async Task PublicActor_StillAllocatesDirectly()
    {
        var (controller, repo) = Setup(RoleNames.PublicActor, "actor");
        await controller.Create(new AllocateViewModel { NeedId = 7, ResourceIds = new() { 1 }, IsOffer = true });
        repo.Verify(r => r.AllocateAsync(7, It.IsAny<List<int>>(), null), Times.Once);
        repo.Verify(r => r.OfferAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task FromMap_ShouldShowAvailableOwnResourcesAndKeepNeed()
    {
        var (controller, repo) = Setup();
        var view = Assert.IsType<ViewResult>(await controller.OfferToNeed(7));
        var model = Assert.IsType<AllocateViewModel>(view.Model);
        Assert.Equal(7, model.NeedId);
        Assert.Equal(1, Assert.Single(model.Resources).Id);
        var next = Assert.IsType<ViewResult>(await controller.Create(new List<int> { 1 }, 7));
        Assert.Equal(7, Assert.IsType<AllocateViewModel>(next.Model).NeedId);
        repo.Verify(r => r.OfferAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task FromMap_ShouldRejectNonProviderAndMissingNeed()
    {
        var (actor, _) = Setup(RoleNames.PublicActor, "actor");
        Assert.IsType<ForbidResult>(await actor.OfferToNeed(7));
        var (provider, _) = Setup();
        Assert.IsType<NotFoundResult>(await provider.OfferToNeed(999));
    }

    [Fact]
    public async Task Provider_NeedWithoutOwner_ShouldRejectOffer()
    {
        var (controller, repo) = Setup(needOwner: null);
        var model = new AllocateViewModel { NeedId = 7, ResourceIds = new() { 1 } };

        var result = await controller.Create(model);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        repo.Verify(r => r.OfferAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        repo.Verify(r => r.AllocateAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task MissingAllocation_ShouldReturnNotFoundWithoutChangingAnything()
    {
        var (controller, repo) = Setup(RoleNames.PublicActor, "actor");

        var approval = await controller.Approve(999, null);
        var removal = await controller.Remove(999, null);

        Assert.IsType<NotFoundResult>(approval);
        Assert.IsType<NotFoundResult>(removal);
        repo.Verify(r => r.ApproveAsync(It.IsAny<int>()), Times.Never);
        repo.Verify(r => r.RemoveAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Admin_ShouldApproveOfferForAnotherOwnersNeed()
    {
        var (controller, repo) = Setup(RoleNames.Admin, "admin");

        var result = await controller.Approve(10, null);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Confirmation", redirect.ActionName);
        Assert.Equal("Need", redirect.ControllerName);
        Assert.Equal(7, redirect.RouteValues!["id"]);
        repo.Verify(r => r.ApproveAsync(10), Times.Once);
    }
}
