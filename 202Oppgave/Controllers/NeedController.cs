using Microsoft.AspNetCore.Mvc;
using _202Oppgave.DataAccess.Repositories;
using Need = _202Oppgave.Models.Need;
using NeedEntity = _202Oppgave.Models.Entities.Need;
using _202Oppgave.ViewModels;

namespace _202Oppgave.Controllers;

public class NeedController : Controller
{
    private readonly INeedRepository _needRepository;

    // Tar imot repositoryet som brukes til å hente og lagre behov i databasen.
    public NeedController(INeedRepository needRepository)
    {
        _needRepository = needRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var needs = await _needRepository.GetAllAsync();
        return View(needs.Select(ToModel).ToList());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new NeedViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NeedViewModel model)
    {
        // Beholder skjemadataene dersom brukeren må rette opp feil.
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var need = new NeedEntity
        {
            Type = model.Type,
            Location = model.Location,
            Priority = model.Priority!.Value,
            Description = model.Description,
            Deadline = model.Deadline!.Value,
            Latitude = model.Latitude!.Value,
            Longitude = model.Longitude!.Value
        };

        await _needRepository.AddAsync(need);

        return RedirectToAction(nameof(Confirmation), new { id = need.Id });
    }

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

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var need = await _needRepository.GetByIdAsync(id);

        if (need is null)
        {
            return NotFound();
        }

        // Fyller redigeringsskjemaet med verdiene som allerede er lagret.
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NeedViewModel model)
    {
        var need = await _needRepository.GetByIdAsync(id);

        if (need is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.NeedId = id;
            return View(model);
        }

        // Oppdaterer det eksisterende behovet med verdiene fra skjemaet.
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var need = await _needRepository.GetByIdAsync(id);

        if (need is null)
        {
            return NotFound();
        }

        // Sletter behovet og sender brukeren tilbake til oversikten.
        await _needRepository.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Geo()
    {
        var needs = await _needRepository.GetAllAsync();

        // Kartet bruker navnet Planlagt for behov som ikke er akutte.
        var data = needs.Select(need => new
        {
            need.Id,
            Kategori = "behov",
            need.Type,
            need.Latitude,
            need.Longitude,
            Prioritet = need.Priority == NeedPriority.Akutt ? "Akutt" : "Planlagt",
            Status = "Registrert",
            Beskrivelse = need.Description
        });

        return Json(data);
    }
    // Gjør databaseobjektet om til modellen som brukes i visningene.
    private static Need ToModel(NeedEntity need)
    {
        return new Need
        {
            Id = need.Id,
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
