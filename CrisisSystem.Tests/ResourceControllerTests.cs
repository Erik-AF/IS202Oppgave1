
using _202Oppgave.Controllers;
using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models.Entities;
using _202Oppgave.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CrisisSystem.Tests;

/// <summary>Tester ResourceController med kontrollerte testdata uten å starte hele applikasjonen.</summary>
public class ResourceControllerTests
{
    /// <summary>Etterligner ressurslagring i minnet slik at controlleren kan testes uten database.</summary>
    private class FakeResourceRepository : IResourceRepository
    {
        public List<Resource> Resources { get; } = new();

        /// <inheritdoc />
        public Task AddAsync(Resource resource)
        {
            resource.Id = Resources.Count + 1;
            Resources.Add(resource);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task UpdateAsync(Resource resource)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<List<Resource>> GetAllAsync()
        {
            return Task.FromResult(Resources.ToList());
        }

        /// <inheritdoc />
        public Task<Resource?> GetByIdAsync(int id)
        {
            return Task.FromResult(
                Resources.FirstOrDefault(r => r.Id == id));
        }

        /// <inheritdoc />
        public Task<List<Resource>> GetByUserIdAsync(string userId)
        {
            return Task.FromResult(
                Resources.Where(r => r.UserId == userId).ToList());
        }

        /// <inheritdoc />
        public Task DeleteAsync(int id)
        {
            Resources.RemoveAll(r => r.Id == id);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<List<Category>> GetCategoriesAsync()
        {
            return Task.FromResult(new List<Category>());
        }
    }

    /// <summary>Oppretter ressurscontroller med en innlogget testbruker og midlertidig filområde.</summary>
    private static ResourceController CreateLoggedInController(FakeResourceRepository repository)
    {
        return new ResourceController(repository, null!)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }, "Test"))
                }
            }
        };
    }

    /// <summary>Gyldige ressursdata lagres med innlogget eier.</summary>
    [Fact]
    public async Task Create_ValidResource_ShouldSaveResource()
    {
        var repository = new FakeResourceRepository();
        var controller = CreateLoggedInController(repository);

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
        Assert.Equal("user-123", saved.UserId);
    }

    /// <summary>Lagring av ressurs sender brukeren til detaljsiden.</summary>
    [Fact]
    public async Task Create_ValidResource_ShouldRedirectToDetails()
    {
        var repository = new FakeResourceRepository();
        var controller = CreateLoggedInController(repository);

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

    /// <summary>Ressursen lagres ikke uten innlogget bruker.</summary>
    [Fact]
    public async Task Create_WithoutLoggedInUser_ShouldNotSave()
    {
        var repository = new FakeResourceRepository();
        var controller = new ResourceController(repository, null!)
        {

            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.Create(new ResourceViewModel());

        Assert.IsType<ChallengeResult>(result);
        Assert.Empty(repository.Resources);
    }

/// <summary>Tekstsøk viser ressurser som passer søket.</summary>
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

    var controller = new ResourceController(repository, null!);

    var result = await controller.Index(
        "generator", null, null, null, null);

    var view = Assert.IsType<ViewResult>(result);
    var model = Assert.IsType<ResourceIndexViewModel>(view.Model);

    var resource = Assert.Single(model.Resources);
    Assert.Equal("Generator", resource.Type);
}

/// <summary>Områdefilteret viser ressurser fra valgt område.</summary>
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

    var controller = new ResourceController(repository, null!);

    var result = await controller.Index(
        null, null, null, "Grimstad", null);

    var view = Assert.IsType<ViewResult>(result);
    var model = Assert.IsType<ResourceIndexViewModel>(view.Model);

    var resource = Assert.Single(model.Resources);
    Assert.Equal("Grimstad", resource.Area);
}

/// <summary>Statusfilteret viser ressurser med valgt status.</summary>
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

    var controller = new ResourceController(repository, null!);

    var result = await controller.Index(
        null, null, "Tilgjengelig", null, null);

    var view = Assert.IsType<ViewResult>(result);
    var model = Assert.IsType<ResourceIndexViewModel>(view.Model);

    var resource = Assert.Single(model.Resources);
    Assert.Equal("Tilgjengelig", resource.Status);
}

/// <summary>Eieren kan oppdatere en eksisterende ressurs.</summary>
[Fact]
public async Task Edit_ExistingResource_ShouldUpdateResource()
{
    var repository = new FakeResourceRepository();
    repository.Resources.Add(new Resource
    {
        Id = 1,
        UserId = "user-123",
        Type = "Generator",
        Name = "Kristiansand"
    });

    var controller = CreateLoggedInController(repository);
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

/// <summary>Redigering av en ukjent ressurs gir 404.</summary>
[Fact]
public async Task Edit_NonExistingResource_ShouldReturnNotFound()
{
    var controller = new ResourceController(
        new FakeResourceRepository(), null!);

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

/// <summary>Eieren kan slette en eksisterende ressurs.</summary>
[Fact]
public async Task Delete_ExistingResource_ShouldRemoveResource()
{
    var repository = new FakeResourceRepository();
    repository.Resources.Add(new Resource
    {
        Id = 1,
        Type = "Generator",
        Name = "Kristiansand",
        UserId = "user-123"
    });

    var controller = CreateLoggedInController(repository);

    var result = await controller.Delete(1);

    Assert.Empty(repository.Resources);

    var redirect = Assert.IsType<RedirectToActionResult>(result);
    Assert.Equal("Index", redirect.ActionName);
}

/// <summary>Sletting av en ukjent ressurs gir 404.</summary>
[Fact]
public async Task Delete_NonExistingResource_ShouldReturnNotFound()
{
    var controller = new ResourceController(
        new FakeResourceRepository(), null!);

    var result = await controller.Delete(999);

    Assert.IsType<NotFoundResult>(result);
}

}
