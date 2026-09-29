using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using _202Oppgave.Models;
using _202Oppgave.ViewModels;

namespace _202Oppgave.Controllers;

/*
 * ResourceController – registrering og visning av ressurser.

 * Dekker kravet om GET/POST-forespørsler og en side som viser registrerte ressurser. 
 */
public class ResourceController : Controller
{
   
    private static readonly List<Resource> _resources = new();

    // GET: /Resource
    // Viser alle registrerte ressurser, som kort med statusfarge.
    public IActionResult Index()
    {
        return View(_resources);
    }

    // GET: /Resource/Create
    // Viser et tomt registreringsskjema.
    public IActionResult Create()
    {
        return View(new ResourceViewModel());
    }

    // POST: /Resource/Create
    // Tar imot utfylt skjema, validerer, og lagrer ressursen.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ResourceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Ugyldig innsending: send brukeren tilbake til samme skjema
            // med feilmeldingene fra valideringsattributtene i ViewModel.
            return View(model);
        }

        var resource = new Resource
        {
            Id = _resources.Count + 1,
            Type = model.Type,
            Description = model.Description,
            Area = model.Area,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            AvailableFrom = model.AvailableFrom,
            AvailableTo = model.AvailableTo,
            ContactName = model.ContactName,
            ContactPhone = model.ContactPhone,
            ContactEmail = model.ContactEmail,
            Status = model.Status,
            OwnerName = model.OwnerName
        };

        _resources.Add(resource);

        // Redirect (ikke bare View) etter POST, så brukeren ikke sender
        // samme skjema på nytt ved refresh.
        return RedirectToAction(nameof(Details), new { id = resource.Id });
    }

    // GET: /Resource/Details/5
    // Viser én registrert ressurs.
    public IActionResult Details(int id)
    {
        var resource = _resources.FirstOrDefault(r => r.Id == id);
        if (resource is null)
        {
            return NotFound();
        }

        return View(resource);
    }

    // GET: /Resource/Geo
    // Returnerer registrerte ressurser med posisjon som JSON.
    [HttpGet]
    public IActionResult Geo()
    {
        var data = _resources
            .Where(r => r.Latitude != null && r.Longitude != null)
            .Select(r => new
            {
                r.Id,
                r.Type,
                r.Area,
                r.Status,
                r.Latitude,
                r.Longitude
            });

        return Json(data);
    }
    
// Sletter en registrert ressurs. 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var resource = _resources.FirstOrDefault(r => r.Id == id);
        if (resource is null)
        {
            return NotFound();
        }
        
        _resources.Remove(resource);

        return RedirectToAction(nameof(Index));
    }
}