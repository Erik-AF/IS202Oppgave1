using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _202Oppgave.DataAccess.Repositories;
using Need = _202Oppgave.Models.Need;
using NeedEntity = _202Oppgave.Models.Entities.Need;
using _202Oppgave.ViewModels;
using _202Oppgave.Models.Authorization;

namespace _202Oppgave.Controllers;

/// <summary>Håndterer registrering, visning og endring av behov med kontroll av brukerens eierskap.</summary>
public class NeedController : Controller
{
    private readonly INeedRepository _needRepository;

    /// <summary>Mottar avhengighetene som NeedController bruker.</summary>
    public NeedController(INeedRepository needRepository)
    {
        _needRepository = needRepository;
    }

    /// <summary>Henter registrerte behov og gjør dem klare for behovsoversikten.</summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var needs = await _needRepository.GetAllAsync();
        return View(needs.Select(ToModel).ToList());
    }

    /// <summary>Viser skjemaet for et nytt behov.</summary>
    [Authorize(Roles = RoleNames.PublicActor + "," + RoleNames.Admin)]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new NeedViewModel());
    }

    /// <summary>Validerer og lagrer behovet med oppretter hentet fra innloggingen, aldri fra skjemadata.</summary>
    [Authorize(Roles = RoleNames.PublicActor + "," + RoleNames.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NeedViewModel model)
    {

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (User.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var need = new NeedEntity
        {
            UserId = userId,
            Type = model.Type,
            Location = model.Location,
            Priority = model.Priority!.Value,
            Description = model.Description,
            Deadline = model.Deadline!.Value,
            Latitude = model.Latitude!.Value,
            Longitude = model.Longitude!.Value,
            GeometryJson =  model.GeometryJson
        };

        await _needRepository.AddAsync(need);

        return RedirectToAction(nameof(Confirmation), new { id = need.Id });
    }

    /// <summary>Viser detaljene for et lagret behov eller returnerer 404 når behovet mangler.</summary>
    [HttpGet]
    public async Task<IActionResult> Confirmation(int id)
    {
        var need = await _needRepository.GetByIdAsync(id);

        if (need is null)
        {
            return NotFound();
        }

        return View(ToModel(need));
    }

    /// <summary>Kontrollerer eierskap og fyller redigeringsskjemaet fra det lagrede behovet.</summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var need = await _needRepository.GetByIdAsync(id);

        if (need is null)
        {
            return NotFound();
        }

        if (!OwnsNeed(need)) return Forbid();

        var model = new NeedViewModel
        {
            Type = need.Type,
            Location = need.Location,
            Priority = need.Priority,
            Description = need.Description,
            Deadline = need.Deadline,
            Latitude = need.Latitude,
            Longitude = need.Longitude
        };

        ViewBag.NeedId = id;
        return View(model);
    }

    /// <summary>Kontrollerer eierskap og validering før eksisterende behov oppdateres.</summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NeedViewModel model)
    {
        var need = await _needRepository.GetByIdAsync(id);

        if (need is null)
        {
            return NotFound();
        }

        if (!OwnsNeed(need)) return Forbid();

        if (!ModelState.IsValid)
        {
            ViewBag.NeedId = id;
            return View(model);
        }

        need.Type = model.Type;
        need.Location = model.Location;
        need.Priority = model.Priority!.Value;
        need.Description = model.Description;
        need.Deadline = model.Deadline!.Value;
        need.Latitude = model.Latitude!.Value;
        need.Longitude = model.Longitude!.Value;

        await _needRepository.UpdateAsync(need);

        return RedirectToAction(nameof(Confirmation), new { id });
    }

    /// <summary>Kontrollerer eierskap og sletter behovet gjennom repositoryet.</summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var need = await _needRepository.GetByIdAsync(id);

        if (need is null)
        {
            return NotFound();
        }

        if (!OwnsNeed(need)) return Forbid();

        await _needRepository.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Returnerer behov med koordinater og kartform for visning i kartet.</summary>
    [HttpGet]
    public async Task<IActionResult> Geo()
    {
        var needs = await _needRepository.GetAllAsync();

        var data = needs.Select(need => new
        {
            need.Id,
            Kategori = "behov",
            need.Type,
            need.Latitude,
            need.Longitude,
            need.GeometryJson,
            Prioritet = need.Priority == NeedPriority.Akutt ? "Akutt" : "Planlagt",
            Status = "Registrert",
            Beskrivelse = need.Description
        });

        return Json(data);
    }
    /// <summary>Sammenligner innlogget bruker med behovets lagrede oppretter.</summary>
    private bool OwnsNeed(NeedEntity need)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return User.Identity?.IsAuthenticated == true
            && !string.IsNullOrWhiteSpace(userId)
            && userId == need.UserId;
    }

    /// <summary>Overfører databasefeltene til modellen som behovsvisningene bruker.</summary>
    private Need ToModel(NeedEntity need)
    {
        return new Need
        {
            Id = need.Id,
            CanManage = OwnsNeed(need),
            Type = need.Type,
            Location = need.Location,
            Priority = need.Priority,
            Description = need.Description,
            Deadline = need.Deadline,
            Latitude = need.Latitude,
            Longitude = need.Longitude
        };
    }
}
