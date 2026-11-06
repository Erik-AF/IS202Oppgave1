
using _202Oppgave.Controllers;
using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models.Entities;
using _202Oppgave.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
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

    [Fact]
    public async Task Create_ValidResource_ShouldSaveResource()
    {
        var repository = new FakeResourceRepository();
        var controller = new ResourceController(repository, null!);

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
        var controller = new ResourceController(repository, null!);

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
}
