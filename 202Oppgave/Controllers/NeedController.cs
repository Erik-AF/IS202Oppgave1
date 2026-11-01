using Microsoft.AspNetCore.Mvc;
using _202Oppgave.Models;
using _202Oppgave.ViewModels;

namespace _202Oppgave.Controllers;

public class NeedController : Controller
{
    // Bruker en liste i minnet, slik Resource gjør. Registreringene forsvinner ved omstart.
    private static readonly List<Need> _needs = new();

    [HttpGet]
    public IActionResult Index()
    {
        return View(_needs);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new NeedViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(NeedViewModel model)
    {
        // Ved feil vises skjemaet med verdiene brukeren allerede har fylt inn.
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Etter validering kopieres skjemadataene til Need, slik at lagringen følger Resource.
        var need = new Need
        {
            Id = _needs.Count + 1,
            Type = model.Type,
            Location = model.Location,
            Priority = model.Priority!.Value,
            Description = model.Description,
            Deadline = model.Deadline!.Value,
            Latitude = model.Latitude!.Value,
            Longitude = model.Longitude!.Value
        };

        _needs.Add(need);
        // Videresender til GET for å unngå dobbeltregistrering ved oppdatering av siden.
        return RedirectToAction(nameof(Confirmation), new { id = need.Id });
    }

    [HttpGet]
    public IActionResult Confirmation(int id)
    {
        var need = _needs.FirstOrDefault(n => n.Id == id);
        if (need is null)
        {
            return NotFound();
        }

        return View(need);
    }

    [HttpGet]
    public IActionResult Geo()
    {
        // Tilpasser dataene til kartet: IkkeAkutt sendes som Planlagt
        // fordi Marius sin fargefunksjon bruker dette navnet.
        var data = _needs.Select(need => new
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
}
