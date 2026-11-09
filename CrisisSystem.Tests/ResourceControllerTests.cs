
using _202Oppgave.Controllers;
using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models.Entities;
using _202Oppgave.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace CrisisSystem.Tests;

public class ResourceControllerTests
{
    private class FakeResourceRepository : IResourceRepository
    {
        public List<Resource> Resources { get; } = new();

        public Task AddAsync(Resource resource)
        {
            resource.Id = Resources.Count + 1;
            Resources.Add(resource);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Resource resource)
        {
            return Task.CompletedTask;
        }

        public Task<List<Resource>> GetAllAsync()
        {
            return Task.FromResult(Resources.ToList());
        }

        public Task<Resource?> GetByIdAsync(int id)
        {
            return Task.FromResult(
                Resources.FirstOrDefault(r => r.Id == id));
        }

        public Task DeleteAsync(int id)
        {
            Resources.RemoveAll(r => r.Id == id);
            return Task.CompletedTask;
        }

        public Task<List<Category>> GetCategoriesAsync()
        {
            return Task.FromResult(new List<Category>());
        }
    }

    private class FakeNeedRepository : INeedRepository
    {
        public List<Need> Needs { get; } = new();

        public Task AddAsync(Need need)
        {
            need.Id = Needs.Count + 1;
            Needs.Add(need);
            return Task.CompletedTask;
        }

        public Task<List<Need>> GetAllAsync()
        {
            return Task.FromResult(Needs.ToList());
        }

        public Task<Need?> GetByIdAsync(int id)
        {
            return Task.FromResult(Needs.FirstOrDefault(n => n.Id == id));
        }

        public Task UpdateAsync(Need need)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            Needs.RemoveAll(n => n.Id == id);
            return Task.CompletedTask;
        }
    }

    // TempData trengs fordi Allocate og Release viser en melding etter redirect
    private class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    private static ResourceController CreateControllerWithTempData(
        FakeResourceRepository repository, FakeNeedRepository needRepository)
    {
        return new ResourceController(repository, needRepository, null!)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), new FakeTempDataProvider())
        };
    }

    [Fact]
    public async Task Create_ValidResource_ShouldSaveResource()
    {
        var repository = new FakeResourceRepository();
        var controller = new ResourceController(repository, new FakeNeedRepository(), null!);

        var model = new ResourceViewModel
        {
            Type = "Generator",
            Area = "Kristiansand",
            Latitude = 58.1467,
            Longitude = 7.9956,
            ContactName = "Testperson",
            ContactPhone = "12345678",
            CategoryId = 1
        };

        await controller.Create(model);

        var saved = Assert.Single(repository.Resources);

        Assert.Equal("Generator", saved.Type);
        Assert.Equal("Kristiansand", saved.Name);
        Assert.Equal(model.Latitude, saved.Latitude);
        Assert.Equal(model.Longitude, saved.Longitude);
        Assert.Equal("Testperson", saved.ContactName);
    }

    [Fact]
    public async Task Create_ValidResource_ShouldRedirectToDetails()
    {
        var repository = new FakeResourceRepository();
        var controller = new ResourceController(repository, new FakeNeedRepository(), null!);

        var model = new ResourceViewModel
        {
            Type = "Drikkevann",
            Area = "Grimstad",
            Latitude = 58.3405,
            Longitude = 8.5934,
            ContactName = "Testperson",
            ContactPhone = "12345678",
            CategoryId = 1
        };

        var result = await controller.Create(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal(repository.Resources[0].Id, redirect.RouteValues!["id"]);
    }
    
[Fact]
public async Task Index_Search_ShouldReturnMatchingResources()
{
    var repository = new FakeResourceRepository();

    repository.Resources.Add(new Resource
    {
        Id = 1,
        Type = "Generator",
        Name = "Kristiansand"
    });

    repository.Resources.Add(new Resource
    {
        Id = 2,
        Type = "Drikkevann",
        Name = "Grimstad"
    });

    var controller = new ResourceController(repository, new FakeNeedRepository(), null!);

    var result = await controller.Index(
        "generator", null, null, null, null);

    var view = Assert.IsType<ViewResult>(result);
    var model = Assert.IsType<ResourceIndexViewModel>(view.Model);

    var resource = Assert.Single(model.Resources);
    Assert.Equal("Generator", resource.Type);
}

[Fact]
public async Task Index_FilterByArea_ShouldReturnCorrectResources()
{
    var repository = new FakeResourceRepository();

    repository.Resources.Add(new Resource
    {
        Id = 1,
        Type = "Generator",
        Name = "Kristiansand"
    });

    repository.Resources.Add(new Resource
    {
        Id = 2,
        Type = "Drikkevann",
        Name = "Grimstad"
    });

    var controller = new ResourceController(repository, new FakeNeedRepository(), null!);

    var result = await controller.Index(
        null, null, null, "Grimstad", null);

    var view = Assert.IsType<ViewResult>(result);
    var model = Assert.IsType<ResourceIndexViewModel>(view.Model);

    var resource = Assert.Single(model.Resources);
    Assert.Equal("Grimstad", resource.Area);
}

[Fact]
public async Task Index_FilterByStatus_ShouldReturnCorrectResources()
{
    var repository = new FakeResourceRepository();

    repository.Resources.Add(new Resource
    {
        Id = 1,
        Type = "Generator",
        Name = "Kristiansand",
        Status = "Tilgjengelig"
    });

    repository.Resources.Add(new Resource
    {
        Id = 2,
        Type = "Drikkevann",
        Name = "Grimstad",
        Status = "Utilgjengelig"
    });

    var controller = new ResourceController(repository, new FakeNeedRepository(), null!);

    var result = await controller.Index(
        null, null, "Tilgjengelig", null, null);

    var view = Assert.IsType<ViewResult>(result);
    var model = Assert.IsType<ResourceIndexViewModel>(view.Model);

    var resource = Assert.Single(model.Resources);
    Assert.Equal("Tilgjengelig", resource.Status);
}

[Fact]
public async Task Edit_ExistingResource_ShouldUpdateResource()
{
    var repository = new FakeResourceRepository();
    repository.Resources.Add(new Resource
    {
        Id = 1,
        Type = "Generator",
        Name = "Kristiansand"
    });

    var controller = new ResourceController(repository, new FakeNeedRepository(), null!);
    var model = new ResourceViewModel
    {
        Type = "Drikkevann",
        Area = "Grimstad",
        Latitude = 58.3405,
        Longitude = 8.5934,
        ContactName = "Testperson",
        ContactPhone = "12345678",
        CategoryId = 1
    };

    var result = await controller.Edit(1, model);

    var updated = Assert.Single(repository.Resources);
    Assert.Equal("Drikkevann", updated.Type);
    Assert.Equal("Grimstad", updated.Name);

    var redirect = Assert.IsType<RedirectToActionResult>(result);
    Assert.Equal("Details", redirect.ActionName);
}

[Fact]
public async Task Edit_NonExistingResource_ShouldReturnNotFound()
{
    var controller = new ResourceController(
        new FakeResourceRepository(), new FakeNeedRepository(), null!);

    var model = new ResourceViewModel
    {
        Type = "Generator",
        Area = "Kristiansand",
        Latitude = 58.1467,
        Longitude = 7.9956,
        ContactName = "Testperson",
        ContactPhone = "12345678",
        CategoryId = 1
    };

    var result = await controller.Edit(999, model);

    Assert.IsType<NotFoundResult>(result);
}

[Fact]
public async Task Delete_ExistingResource_ShouldRemoveResource()
{
    var repository = new FakeResourceRepository();
    repository.Resources.Add(new Resource
    {
        Id = 1,
        Type = "Generator",
        Name = "Kristiansand"
    });

    var controller = new ResourceController(repository, new FakeNeedRepository(), null!);

    var result = await controller.Delete(1);

    Assert.Empty(repository.Resources);

    var redirect = Assert.IsType<RedirectToActionResult>(result);
    Assert.Equal("Index", redirect.ActionName);
}

[Fact]
public async Task Delete_NonExistingResource_ShouldReturnNotFound()
{
    var controller = new ResourceController(
        new FakeResourceRepository(), new FakeNeedRepository(), null!);

    var result = await controller.Delete(999);

    Assert.IsType<NotFoundResult>(result);
}

[Fact]
public async Task Allocate_SelectedResources_ShouldBeReservedForNeed()
{
    var repository = new FakeResourceRepository();
    repository.Resources.Add(new Resource { Id = 1, Type = "Generator", Name = "Kristiansand" });
    repository.Resources.Add(new Resource { Id = 2, Type = "Traktor", Name = "Grimstad" });
    repository.Resources.Add(new Resource { Id = 3, Type = "Båt", Name = "Arendal" });

    var needRepository = new FakeNeedRepository();
    needRepository.Needs.Add(new Need { Id = 10, Type = "Strøm", Location = "Vennesla" });

    var controller = CreateControllerWithTempData(repository, needRepository);

    var result = await controller.Allocate(10, new List<int> { 1, 2 });

    Assert.All(repository.Resources.Where(r => r.Id is 1 or 2), r =>
    {
        Assert.Equal(10, r.NeedId);
        Assert.Equal("Reservert", r.Status);
    });

    // Ressursen som ikke var valgt, skal ikke røres
    var notSelected = repository.Resources.Single(r => r.Id == 3);
    Assert.Null(notSelected.NeedId);
    Assert.Equal("Tilgjengelig", notSelected.Status);

    var redirect = Assert.IsType<RedirectToActionResult>(result);
    Assert.Equal("Index", redirect.ActionName);
}

[Fact]
public async Task Allocate_AlreadyAllocatedResource_ShouldMoveToNewNeed()
{
    var oldNeed = new Need { Id = 10, Type = "Strøm", Location = "Vennesla" };
    var newNeed = new Need { Id = 20, Type = "Transport", Location = "Lillesand" };

    var needRepository = new FakeNeedRepository();
    needRepository.Needs.Add(oldNeed);
    needRepository.Needs.Add(newNeed);

    var repository = new FakeResourceRepository();
    repository.Resources.Add(new Resource
    {
        Id = 1, Type = "Generator", Name = "Kristiansand",
        NeedId = 10, Need = oldNeed, Status = "Reservert"
    });

    var controller = CreateControllerWithTempData(repository, needRepository);

    await controller.Allocate(20, new List<int> { 1 });

    var resource = Assert.Single(repository.Resources);
    Assert.Equal(20, resource.NeedId);
    Assert.Same(newNeed, resource.Need);
}

[Fact]
public async Task Allocate_NonExistingNeed_ShouldReturnNotFound()
{
    var repository = new FakeResourceRepository();
    repository.Resources.Add(new Resource { Id = 1, Type = "Generator", Name = "Kristiansand" });

    var controller = CreateControllerWithTempData(repository, new FakeNeedRepository());

    var result = await controller.Allocate(999, new List<int> { 1 });

    Assert.IsType<NotFoundResult>(result);
    Assert.Null(repository.Resources[0].NeedId);
}

[Fact]
public async Task Release_AllocatedResource_ShouldBecomeAvailable()
{
    var need = new Need { Id = 10, Type = "Strøm", Location = "Vennesla" };

    var repository = new FakeResourceRepository();
    repository.Resources.Add(new Resource
    {
        Id = 1, Type = "Generator", Name = "Kristiansand",
        NeedId = 10, Need = need, Status = "Reservert"
    });

    var controller = CreateControllerWithTempData(repository, new FakeNeedRepository());

    var result = await controller.Release(1);

    var resource = Assert.Single(repository.Resources);
    Assert.Null(resource.NeedId);
    Assert.Null(resource.Need);
    Assert.Equal("Tilgjengelig", resource.Status);

    var redirect = Assert.IsType<RedirectToActionResult>(result);
    Assert.Equal("Index", redirect.ActionName);
    Assert.Equal("Need", redirect.ControllerName);
}

[Fact]
public async Task Release_NonExistingResource_ShouldReturnNotFound()
{
    var controller = CreateControllerWithTempData(new FakeResourceRepository(), new FakeNeedRepository());

    var result = await controller.Release(999);

    Assert.IsType<NotFoundResult>(result);
}

}
