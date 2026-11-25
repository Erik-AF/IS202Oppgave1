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
using System.Security.Claims;

using ResourceModel = _202Oppgave.Models.Resource;

namespace _202Oppgave.Controllers;

public class ResourceController : Controller
{
    private readonly IResourceRepository _repo;

    private readonly IWebHostEnvironment _env;

    private const string UploadFolder = "uploads/ressurser";

    public ResourceController(IResourceRepository repo, IWebHostEnvironment env)
    {
        _repo = repo;
        _env = env;
    }

    /// Filtrerer og sorterer ressursene ut fra søk, kategori, status og område.
    [Authorize(Roles = RoleNames.PublicActor + "," + RoleNames.Admin)]
    public async Task<IActionResult> Index(string? q, string? category, string? status, string? area, string? sort)
    {
        var entities = await _repo.GetAllAsync();
        var all = entities.Select(ToModel).ToList();

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

        model.Resources = FilterResources(all, q, category, status, area, sort).ToList();
        return View(model);
    }

    /// Eksporterer tilgjengelige ressurser som passer filtrene til en XLSX-fil.
    [HttpGet]
    [Authorize(Roles = RoleNames.PublicActor + "," + RoleNames.Admin)]
    public async Task<IActionResult> ExportExcel(string? q, string? category, string? status, string? area, string? sort)
    {
        var all = (await _repo.GetAllAsync()).Select(ToModel);
        var resources = FilterResources(all, q, category, status, area, sort)
            .Where(r => r.Status == "Tilgjengelig").ToList();
        var exportedAt = DateTimeOffset.UtcNow;
        var bytes = _202Oppgave.Services.ResourceExcelExport.Create(resources, exportedAt);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ressurser-{exportedAt:yyyyMMdd-HHmmss}.xlsx");
    }

    /// Bruker samme søk, filterverdier og sortering for ressursoversikten og eksporten.
    private static IEnumerable<ResourceModel> FilterResources(IEnumerable<ResourceModel> all,
        string? q, string? category, string? status, string? area, string? sort)
    {

        IEnumerable<Models.Resource> result = all;

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

        result = sort switch
        {
            "category" => result.OrderBy(r => r.CategoryName),
            "area" => result.OrderBy(r => r.Area),
            _ => result.OrderByDescending(r => r.Id)
        };

        return result;
    }

    [Authorize(Roles = RoleNames.ResourceProvider + "," + RoleNames.Admin)]
    public async Task<IActionResult> Create()
    {
        var model = new ResourceViewModel();
        await FillCategoryOptionsAsync(model);
        return View(model);
    }

    /// Validerer ressursen, lagrer eventuell bildefil og knytter oppføringen til innlogget bruker.
    [Authorize(Roles = RoleNames.ResourceProvider + "," + RoleNames.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResourceViewModel model)
    {

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (User.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }
        if (!ModelState.IsValid)
        {
            await FillCategoryOptionsAsync(model);
            return View(model);
        }

        var resource = new ResourceEntity
        {
            UserId = userId,
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

        if (model.Image != null)
        {
            resource.ImagePath = await SaveImageAsync(model.Image);
        }

        await _repo.AddAsync(resource);

        return RedirectToAction(
            nameof(Details),
            new { id = resource.Id });
    }

    [Authorize(Roles = RoleNames.ResourceProvider + "," + RoleNames.Admin + "," + RoleNames.PublicActor)]
    public async Task<IActionResult> Details(int id)
    {
        var entity = await _repo.GetByIdAsync(id);

        if (entity == null)
        {
            return NotFound();
        }

        var model = ToModel(entity);

        model.CanManage = CanManageResource(entity);

        return View(model);
    }

    /// Kontrollerer tilgang og fyller redigeringsskjemaet med eksisterende ressursdata.
    [Authorize(Roles = RoleNames.ResourceProvider + "," + RoleNames.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _repo.GetByIdAsync(id);

        if (entity == null)
        {
            return NotFound();
        }

        if (!CanManageResource(entity)) return Forbid();

        var model = new ResourceViewModel
        {
            Type = entity.Type,
            Description = entity.Description,
            Area = entity.Name,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,

            GeometryJson = entity.GeometryJson,
            AvailableFrom = entity.AvailableFrom,
            AvailableTo = entity.AvailableTo,
            ContactName = entity.ContactName,
            ContactPhone = entity.ContactPhone,
            ContactEmail = entity.ContactEmail,
            Status = entity.Status,
            OwnerName = entity.User?.FullName ?? entity.OwnerName,
            ImagePath = entity.ImagePath,
            CategoryId = entity.CategoryId

        };
        ViewBag.ResourceID = id;
        await FillCategoryOptionsAsync(model);
        return View(model);
    }

    /// Returnerer ressursenes posisjoner og kartformer som JSON.
    [Authorize(Roles = RoleNames.ResourceProvider + "," + RoleNames.Admin + "," + RoleNames.PublicActor)]
    [HttpGet]
    public async Task<IActionResult> Geo()
    {
        var resources = await _repo.GetAllAsync();

        if (User.IsInRole(RoleNames.ResourceProvider) &&
            !User.IsInRole(RoleNames.Admin) &&
            !User.IsInRole(RoleNames.PublicActor))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            resources = resources
                .Where(r => r.UserId == userId)
                .ToList();
        }

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
                CategoryName = r.Category != null ? r.Category.Name : null
            });

        return Json(data);
    }

    /// Kontrollerer tilgang før ressursen og det tilhørende bildet slettes.
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

        if (!CanManageResource(resource)) return Forbid();

        DeleteImage(resource.ImagePath);

        await _repo.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }

    /// Sammenligner innlogget bruker med ressursens lagrede eier.
    private bool OwnsResource(ResourceEntity resource)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return User.Identity?.IsAuthenticated == true
               && !string.IsNullOrWhiteSpace(userId)
               && userId == resource.UserId;
    }

    /// Tillater ressursens eier eller administrator å endre ressursen.
    private bool CanManageResource(ResourceEntity resource)
    {
        return OwnsResource(resource) || User.IsInRole(RoleNames.Admin);
    }

    /// Bygger visningsdata og foretrekker kontaktinformasjon fra eierens profil fremfor eldre ressursfelter.
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

            ContactName = entity.User?.FullName ?? entity.ContactName,
            ContactPhone = entity.User?.PhoneNumber ?? entity.ContactPhone,
            ContactEmail = entity.User?.Email ?? entity.ContactEmail,
            Status = entity.Status,
            OwnerName = entity.User?.FullName ?? entity.OwnerName,
            ImagePath = entity.ImagePath,
            GeometryJson = entity.GeometryJson,
            CategoryName = entity.Category?.Name
        };
    }

    /// Kontrollerer tilgang og validering før ressursen og eventuelt bildet oppdateres.
    [Authorize(Roles = RoleNames.ResourceProvider + "," + RoleNames.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ResourceViewModel model)
    {

        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        if (!CanManageResource(entity)) return Forbid();

        if (!ModelState.IsValid)
        {
            ViewBag.ResourceId = id;

            model.ImagePath = entity.ImagePath;
            await FillCategoryOptionsAsync(model);
            return View(model);
        }

        if (model.Image != null)
        {
            DeleteImage(entity.ImagePath);
            entity.ImagePath = await SaveImageAsync(model.Image);
        }

        entity.Type = model.Type;
        entity.Description = model.Description;
        entity.Name = model.Area;
        entity.Latitude = model.Latitude;
        entity.Longitude = model.Longitude;
        entity.GeometryJson = model.GeometryJson;
        entity.AvailableFrom = model.AvailableFrom;
        entity.AvailableTo = model.AvailableTo;
        entity.Status = model.Status;
        entity.CategoryId = model.CategoryId;

        await _repo.UpdateAsync(entity);

        return RedirectToAction(nameof(Details), new { id });
    }

    /// Lagrer opplastet bilde med et unikt filnavn og returnerer stien som visningen kan bruke.
    private async Task<string> SaveImageAsync(IFormFile image)
    {
        var folder = Path.Combine(_env.WebRootPath, UploadFolder);
        Directory.CreateDirectory(folder);

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(folder, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await image.CopyToAsync(stream);

        return $"/{UploadFolder}/{fileName}";
    }

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

    /// Fyller kategorivalgene fra databasen, også når skjemaet må vises igjen etter feil.
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