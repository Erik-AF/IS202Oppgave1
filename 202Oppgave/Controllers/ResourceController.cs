using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models;
using _202Oppgave.Models.Authorization;
using _202Oppgave.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using ResourceEntity = _202Oppgave.Models.Entities.Resource;
using Microsoft.AspNetCore.Mvc.Rendering;

// Skiller mellom den gamle modellen og database-entity
using ResourceModel = _202Oppgave.Models.Resource;

namespace _202Oppgave.Controllers;


 ///ResourceController – registrering og visning av ressurser.
 
public class ResourceController : Controller
{
    private readonly IResourceRepository _repo;

// Gir tilgang til wwwroot-mappen, der opplastede bilder lagres
    private readonly IWebHostEnvironment _env;

// Mappen bildene lagres i, under wwwroot
    private const string UploadFolder = "uploads/ressurser";

// Dependency Injection
    public ResourceController(IResourceRepository repo, IWebHostEnvironment env)
    {
        _repo = repo;
        _env = env;
    }
    // GET: /Resource
    // Henter alle ressurser fra MariaDB
    // ResourceProvider og Admin kan åpne Create
    [Authorize(Roles = RoleNames.PublicActor + "," + RoleNames.Admin)]
    public async Task<IActionResult> Index(string? q, string? category, string? status, string? area, string? sort)
    {
        var entities = await _repo.GetAllAsync();
        var all = entities.Select(ToModel).ToList();

        // Valgene i nedtrekkslistene lages fra ALLE ressursene, før filtrering
        var model = new ResourceIndexViewModel
        {
            Q = q,
            Category = category,
            Status = status,
            Area = area,
            Sort = sort,
            CategoryOptions = all
                .Where(r => r.CategoryName != null)
                .Select(r => r.CategoryName!)
                .Distinct()
                .OrderBy(c => c)
                .ToList(),
            StatusOptions = all.Select(r => r.Status).Distinct().OrderBy(s => s).ToList(),
            AreaOptions = all.Select(r => r.Area).Distinct().OrderBy(a => a).ToList()
        };

        // Starter med alle, og fjerner det som ikke passer for hvert filter
        IEnumerable<Models.Resource> result = all;

        // Søk i type, beskrivelse, sted og eier
        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim().ToLower();
            result = result.Where(r =>
                r.Type.ToLower().Contains(search) ||
                (r.Description ?? "").ToLower().Contains(search) ||
                r.Area.ToLower().Contains(search) ||
                (r.OwnerName ?? "").ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            result = result.Where(r => r.CategoryName == category);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            result = result.Where(r => r.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(area))
        {
            result = result.Where(r => r.Area == area);
        }

        // Sortering. Standard er nyeste først (høyest Id).
        result = sort switch
        {
            "category" => result.OrderBy(r => r.CategoryName),
            "area" => result.OrderBy(r => r.Area),
            _ => result.OrderByDescending(r => r.Id)
        };

        model.Resources = result.ToList();
        return View(model);
    }

    // GET: /Resource/Create
    // Viser et tomt registreringsskjema.
    // ResourceProvider og Admin kan åpne Create
    [Authorize(Roles = RoleNames.ResourceProvider + "," + RoleNames.Admin)]
    public async Task<IActionResult> Create()
    {
        var model = new ResourceViewModel();
        await FillCategoryOptionsAsync(model);   
        return View(model);
    }

    // POST: /Resource/Create
    // Lagrer en ny ressurs i MariaDB
    // ResourceProvider og Admin kan åpne Create
    [Authorize(Roles = RoleNames.ResourceProvider + "," + RoleNames.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResourceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await FillCategoryOptionsAsync(model);   
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
            GeometryJson = model.GeometryJson,
            AvailableFrom = model.AvailableFrom,
            AvailableTo = model.AvailableTo,
            ContactName = model.ContactName,
            ContactPhone = model.ContactPhone,
            ContactEmail = model.ContactEmail,
            Status = model.Status,
            OwnerName = model.OwnerName,
            CategoryId = model.CategoryId

        };

        // Lagrer bildet på disk hvis brukeren har valgt et
        if (model.Image != null)
        {
            resource.ImagePath = await SaveImageAsync(model.Image);
        }
        
        //Lagrer ressursen i MariaDB
        await _repo.AddAsync(resource);

        // Sender brukeren til detaljsiden
        return RedirectToAction(
            nameof(Details),
            new { id = resource.Id });
    }


    // GET: /Resource/Details/5
    // Henter én ressurs fra MariaDB
    [Authorize(Roles = RoleNames.ResourceProvider + "," + RoleNames.Admin + "," + RoleNames.PublicActor)]
    public async Task<IActionResult> Details(int id)
    {
        var entity = await _repo.GetByIdAsync(id);

        if (entity == null)
        {
            return NotFound();
        }

        return View(ToModel(entity));
    }

    //GET: /Resource/Edit/5
    // Henter ressursen fra Maria DB og viser skjeamet ferdig utfylt. 
    [Authorize(Roles = RoleNames.ResourceProvider + "," + RoleNames.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        
        if (entity == null)
            {
            return NotFound();
            }
        //Kopierer verdier fra databsen over i skjemaet
        var model = new ResourceViewModel
        {
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
            OwnerName = entity.OwnerName,
            ImagePath = entity.ImagePath,
            CategoryId = entity.CategoryId

            
        };
        ViewBag.ResourceID = id;
        await FillCategoryOptionsAsync(model);       
        return View(model);
    }


    // GET: /Resource/Geo
    // Henter kartdata fra MariaDB
    [Authorize(Roles = RoleNames.ResourceProvider + "," + RoleNames.Admin + "," + RoleNames.PublicActor)]
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
                r.Longitude,
                r.GeometryJson,
            });

        return Json(data);
    }

    // POST: /Resource/Delete
    // Sletter ressurs fra MariaDB
    [Authorize(Roles = RoleNames.ResourceProvider + "," + RoleNames.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var resource = await _repo.GetByIdAsync(id);

        if (resource == null)
        {
            return NotFound();
        }
        
        // Sletter bildefilen også, så den ikke blir liggende igjen
        DeleteImage(resource.ImagePath);

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
            OwnerName = entity.OwnerName,
            ImagePath = entity.ImagePath,
            CategoryName = entity.Category?.Name
        };
    }
    // POST: /Resource/Edit/5
    // Lagrer endringene i MariaDB.
    [Authorize(Roles = RoleNames.ResourceProvider + "," + RoleNames.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ResourceViewModel model)
    {
        // Ugyldig skjema: vis det på nytt med feilmeldinger
        if (!ModelState.IsValid)
        {
            ViewBag.ResourceId = id;
            // Viser dagens bilde igjen, siden det ikke sendes med skjemaet
            model.ImagePath = (await _repo.GetByIdAsync(id))?.ImagePath;
            await FillCategoryOptionsAsync(model);   //
            return View(model);
        }

        // Hent den ekte ressursen fra databasen
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
        {
            return NotFound();
        }
        
        // Nytt bilde valgt: lagre det nye og slett det gamle.
        // Ikke valgt: behold bildet som allerede er lagret.
        if (model.Image != null)
        {
            DeleteImage(entity.ImagePath);
            entity.ImagePath = await SaveImageAsync(model.Image);
        }

        // Kopier verdiene fra skjemaet over i ressursen
        entity.Type = model.Type;
        entity.Description = model.Description;
        entity.Name = model.Area;
        entity.Latitude = model.Latitude;
        entity.Longitude = model.Longitude;
        entity.AvailableFrom = model.AvailableFrom;
        entity.AvailableTo = model.AvailableTo;
        entity.ContactName = model.ContactName;
        entity.ContactPhone = model.ContactPhone;
        entity.ContactEmail = model.ContactEmail;
        entity.Status = model.Status;
        entity.OwnerName = model.OwnerName;
        entity.CategoryId = model.CategoryId;

        // Lagre i MariaDB
        await _repo.UpdateAsync(entity);

        return RedirectToAction(nameof(Details), new { id });
    }
    // Lagrer et opplastet bilde i wwwroot/uploads/ressurser.
    // Filen får et tilfeldig navn (Guid), så to bilder med samme navn
    // ikke overskriver hverandre.
    // Returnerer stien som lagres i databasen.
    private async Task<string> SaveImageAsync(IFormFile image)
    {
        var folder = Path.Combine(_env.WebRootPath, UploadFolder);
        Directory.CreateDirectory(folder); // lager mappen første gang

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(folder, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await image.CopyToAsync(stream);

        return $"/{UploadFolder}/{fileName}";
    }
    
    // Sletter et opplastet bilde fra disk.
    // Sletter bare filer i uploads-mappen, aldri standardbildene.
    private void DeleteImage(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath) ||
            !imagePath.StartsWith($"/{UploadFolder}/"))
        {
            return;
        }

        var fullPath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/'));
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
    
    // Fyller nedtrekkslisten med kategorier fra databasen.
    // Brukes både når skjemaet vises første gang og når det vises på nytt med feil.
    private async Task FillCategoryOptionsAsync(ResourceViewModel model)
    {
        var categories = await _repo.GetCategoriesAsync();

        model.CategoryOptions = categories
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToList();
    }
}