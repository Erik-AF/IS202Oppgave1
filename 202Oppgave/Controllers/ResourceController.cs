using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models;
using _202Oppgave.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

// Skiller mellom den gamle modellen og database-entity
using ResourceModel = _202Oppgave.Models.Resource;
using ResourceEntity = _202Oppgave.Models.Entities.Resource;

namespace _202Oppgave.Controllers;

/*
 * ResourceController – registrering og visning av ressurser.

 * Dekker kravet om GET/POST-forespørsler og en side som viser registrerte ressurser. 
 */
public class ResourceController : Controller
{
    private readonly IResourceRepository _repo;

    // Dependency Injection
    public ResourceController(IResourceRepository repo)
    {
        _repo = repo;

    }

    private static readonly List<Resource> _resources = new();

    // GET: /Resource
    // Henter alle ressurser fra MariaDB
    public async Task<IActionResult> Index()
    {
        var entities = await _repo.GetAllAsync();

        var resources = entities.Select(ToModel).ToList();

        return View(resources);
    }

    // GET: /Resource/Create
    // Viser et tomt registreringsskjema.
    public IActionResult Create()
    {
        return View(new ResourceViewModel());
    }

    // POST: /Resource/Create
    // Lagrer en ny ressurs i MariaDB
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResourceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Konverterer ViewModel til database-entity
        var resource = new ResourceEntity
        {
            Type = model.Type,
            Description = model.Description,
            Name = model.Area,
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

        // Lagrer i MariaDB
        await _repo.AddAsync(resource);

        // Sender brukeren til detaljsiden
        return RedirectToAction(
            nameof(Details),
            new { id = resource.Id });
    }


    // GET: /Resource/Details/5
    // Henter én ressurs fra MariaDB
    public async Task<IActionResult> Details(int id)
    {
        var entity = await _repo.GetByIdAsync(id);

        if (entity == null)
        {
            return NotFound();
        }

        return View(ToModel(entity));
    }

    // GET: /Resource/Geo
    // Henter kartdata fra MariaDB
    [HttpGet]
    public async Task<IActionResult> Geo()
    {
        var resources = await _repo.GetAllAsync();

        var data = resources
            .Where(r => r.Latitude != null &&
                        r.Longitude != null)
            .Select(r => new
            {
                r.Id,
                r.Type,
                Area = r.Name,
                r.Status,
                r.Latitude,
                r.Longitude
            });

        return Json(data);
    }

    // POST: /Resource/Delete
    // Sletter ressurs fra MariaDB
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var resource = await _repo.GetByIdAsync(id);

        if (resource == null)
        {
            return NotFound();
        }

        await _repo.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }
    // Konverterer database-entity til modellen
    // som MVC Views allerede bruker
    private static ResourceModel ToModel(ResourceEntity entity)
    {
        return new ResourceModel
        {
            Id = entity.Id,
            Type = entity.Type,
            Description = entity.Description,
            Area = entity.Name,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            AvailableFrom = entity.AvailableFrom,
            AvailableTo = entity.AvailableTo,
            ContactName = entity.ContactName,
            ContactPhone = entity.ContactPhone,
            ContactEmail = entity.ContactEmail,
            Status = entity.Status,
            OwnerName = entity.OwnerName
        };
    }
}