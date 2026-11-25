using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using _202Oppgave.Controllers;
using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models.Entities;
using _202Oppgave.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CrisisSystem.Tests;

/// <summary>Tester NeedController med kontrollerte testdata uten å starte hele applikasjonen.</summary>
public class NeedControllerTests
{
    /// <summary>Etterligner behovslagring i minnet slik at controlleren kan testes uten database.</summary>
    private class FakeNeedRepository : INeedRepository
    {
        public List<Need> Needs { get; } = new();

        /// <inheritdoc />
        public Task AddAsync(Need need)
        {
            need.Id = Needs.Count + 1;
            Needs.Add(need);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<List<Need>> GetAllAsync()
        {
            return Task.FromResult(Needs.ToList());
        }

        /// <inheritdoc />
        public Task<Need?> GetByIdAsync(int id)
        {
            return Task.FromResult(
                Needs.FirstOrDefault(n => n.Id == id));
        }

        /// <inheritdoc />
        public Task UpdateAsync(Need need)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteAsync(int id)
        {
            Needs.RemoveAll(n => n.Id == id);
            return Task.CompletedTask;
        }
    }

    /// <summary>Registreringssiden starter med et tomt behovsskjema.</summary>
    [Fact]
    public void Create_Get_ShouldReturnEmptyForm()
    {
        var controller = new NeedController(
            new FakeNeedRepository());

        var result = controller.Create();

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<NeedViewModel>(view.Model);
    }

    /// <summary>Ugyldige behovsdata viser skjemaet igjen.</summary>
    [Fact]
    public async Task Create_InvalidModel_ShouldReturnView()
    {
        var repository = new FakeNeedRepository();
        var controller = CreateController(repository);
        var model = new NeedViewModel();

        controller.ModelState.AddModelError(
            "Type", "Type er obligatorisk");

        var result = await controller.Create(model);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(model, view.Model);
        Assert.Empty(repository.Needs);
    }

    /// <summary>Gyldige behovsdata lagres med innlogget oppretter.</summary>
    [Fact]
    public async Task Create_ValidModel_ShouldSaveNeed()
    {
        var repository = new FakeNeedRepository();
        var controller = CreateController(repository);

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

        Assert.Equal("user-123", savedNeed.UserId);
        Assert.Equal(model.Type, savedNeed.Type);
        Assert.Equal(model.Location, savedNeed.Location);
        Assert.Equal(model.Priority, savedNeed.Priority);
        Assert.Equal(model.Description, savedNeed.Description);
        Assert.Equal(model.Deadline, savedNeed.Deadline);
        Assert.Equal(model.Latitude, savedNeed.Latitude);
        Assert.Equal(model.Longitude, savedNeed.Longitude);
    }

    /// <summary>Lagring av behov sender brukeren til bekreftelsen.</summary>
    [Fact]
    public async Task Create_ValidModel_ShouldRedirectToConfirmation()
    {
        var repository = new FakeNeedRepository();
        var controller = CreateController(repository);

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

/// <summary>Et eksisterende behov oppdateres ved gyldig redigering.</summary>
[Fact]
public async Task Edit_ExistingNeed_ShouldUpdateNeed()
{
    var repository = new FakeNeedRepository();
    var need = new Need
    {
        Id = 1,
        UserId = "user-123",
        Type = "Mat",
        Location = "Kristiansand",
        Priority = NeedPriority.IkkeAkutt,
        Description = "Behov for mat",
        Deadline = DateTime.Today.AddDays(2),
        Latitude = 58.1467,
        Longitude = 7.9956
    };
    repository.Needs.Add(need);

    var controller = CreateController(repository);
    var model = new NeedViewModel
    {
        Type = "Drikkevann",
        Location = "Grimstad",
        Priority = NeedPriority.Akutt,
        Description = "Akutt behov for vann",
        Deadline = DateTime.Today.AddDays(1),
        Latitude = 58.3405,
        Longitude = 8.5934
    };

    var result = await controller.Edit(1, model);

    Assert.Equal("user-123", repository.Needs[0].UserId);
    Assert.Equal("Drikkevann", repository.Needs[0].Type);
    Assert.Equal("Grimstad", repository.Needs[0].Location);
    Assert.Equal(NeedPriority.Akutt, repository.Needs[0].Priority);

    var redirect = Assert.IsType<RedirectToActionResult>(result);
    Assert.Equal("Confirmation", redirect.ActionName);
}

/// <summary>Redigering av et ukjent behov gir 404.</summary>
[Fact]
public async Task Edit_NonExistingNeed_ShouldReturnNotFound()
{
    var controller = new NeedController(new FakeNeedRepository());

    var result = await controller.Edit(999);

    Assert.IsType<NotFoundResult>(result);
}

/// <summary>Et eksisterende behov fjernes ved sletting.</summary>
[Fact]
public async Task Delete_ExistingNeed_ShouldRemoveNeed()
{
    var repository = new FakeNeedRepository();
    repository.Needs.Add(new Need { Id = 1, UserId = "user-123" });

    var controller = CreateController(repository);

    var result = await controller.Delete(1);

    Assert.Empty(repository.Needs);

    var redirect = Assert.IsType<RedirectToActionResult>(result);
    Assert.Equal("Index", redirect.ActionName);
}

/// <summary>Sletting av et ukjent behov gir 404.</summary>
[Fact]
public async Task Delete_NonExistingNeed_ShouldReturnNotFound()
{
    var controller = new NeedController(new FakeNeedRepository());

    var result = await controller.Delete(999);

    Assert.IsType<NotFoundResult>(result);
}

    /// <summary>Oppretter controlleren med repository og innlogget testidentitet.</summary>
    private static NeedController CreateController(INeedRepository repository)
    {
        return new NeedController(repository)
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

    /// <summary>Behov lagres ikke uten en gyldig innlogget identitet.</summary>
    [Theory]
    [InlineData(false, "user-123")]
    [InlineData(true, null)]
    [InlineData(true, " ")]
    public async Task Create_WithoutValidIdentity_ShouldNotSave(bool authenticated, string? id)
    {
        var repository = new FakeNeedRepository();
        var controller = CreateController(repository);
        var claims = id is null ? Array.Empty<Claim>() : new[] { new Claim(ClaimTypes.NameIdentifier, id) };
        controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticated ? "Test" : null));

        var result = await controller.Create(new NeedViewModel());

        Assert.IsType<ChallengeResult>(result);
        Assert.Empty(repository.Needs);
    }

    /// <summary>Begge registreringsendepunktene krever autorisasjon.</summary>
    [Fact]
    public void Create_Endpoints_ShouldRequireAuthorization()
    {
        var actions = typeof(NeedController).GetMethods().Where(m => m.Name == "Create").ToList();
        Assert.Equal(2, actions.Count);
        Assert.All(actions, method => Assert.NotEmpty(method.GetCustomAttributes(typeof(AuthorizeAttribute), true)));
        Assert.Null(typeof(NeedViewModel).GetProperty("UserId"));
    }
}