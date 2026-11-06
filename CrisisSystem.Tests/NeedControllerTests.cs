using _202Oppgave.Controllers;
using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models.Entities;
using _202Oppgave.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CrisisSystem.Tests;

public class NeedControllerTests
{
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
            return Task.FromResult(
                Needs.FirstOrDefault(n => n.Id == id));
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

    [Fact]
    public void Create_Get_ShouldReturnEmptyForm()
    {
        var controller = new NeedController(
            new FakeNeedRepository());

        var result = controller.Create();

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<NeedViewModel>(view.Model);
    }

    [Fact]
    public async Task Create_InvalidModel_ShouldReturnView()
    {
        var repository = new FakeNeedRepository();
        var controller = new NeedController(repository);
        var model = new NeedViewModel();

        controller.ModelState.AddModelError(
            "Type", "Type er obligatorisk");

        var result = await controller.Create(model);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(model, view.Model);
        Assert.Empty(repository.Needs);
    }
    
    [Fact]
    public async Task Create_ValidModel_ShouldSaveNeed()
    {
        var repository = new FakeNeedRepository();
        var controller = new NeedController(repository);

        var model = new NeedViewModel
        {
            Type = "Drikkevann",
            Location = "Kristiansand",
            Priority = NeedPriority.Akutt,
            Description = "Behov for drikkevann",
            Deadline = DateTime.Today.AddDays(2),
            Latitude = 58.1467,
            Longitude = 7.9956
        };

        await controller.Create(model);

        var savedNeed = Assert.Single(repository.Needs);

        Assert.Equal(model.Type, savedNeed.Type);
        Assert.Equal(model.Location, savedNeed.Location);
        Assert.Equal(model.Priority, savedNeed.Priority);
        Assert.Equal(model.Description, savedNeed.Description);
        Assert.Equal(model.Deadline, savedNeed.Deadline);
        Assert.Equal(model.Latitude, savedNeed.Latitude);
        Assert.Equal(model.Longitude, savedNeed.Longitude);
    }

    [Fact]
    public async Task Create_ValidModel_ShouldRedirectToConfirmation()
    {
        var repository = new FakeNeedRepository();
        var controller = new NeedController(repository);

        var model = new NeedViewModel
        {
            Type = "Mat",
            Location = "Kristiansand",
            Priority = NeedPriority.IkkeAkutt,
            Description = "Behov for mat",
            Deadline = DateTime.Today.AddDays(3),
            Latitude = 58.1467,
            Longitude = 7.9956
        };

        var result = await controller.Create(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Confirmation", redirect.ActionName);
        Assert.Equal(repository.Needs[0].Id, redirect.RouteValues!["id"]);
    }

}