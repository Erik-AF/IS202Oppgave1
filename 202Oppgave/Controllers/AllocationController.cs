using _202Oppgave.Services;
using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models.Authorization;
using _202Oppgave.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ResourceModel = _202Oppgave.Models.Resource;
using ResourceEntity = _202Oppgave.Models.Entities.Resource;

namespace _202Oppgave.Controllers;

/*
 * AllocationController – allokerer valgte ressurser til et behov.
 * Bare offentlige aktører og admin kan allokere, samme roller som ser ressurslisten.
 */
[Authorize(Roles = RoleNames.PublicActor + "," + RoleNames.Admin)]
public class AllocationController : Controller
{
    private readonly IAllocationRepository _allocationRepo;
    private readonly IResourceRepository _resourceRepo;
    private readonly INeedRepository _needRepo;

    // Dependency Injection: får alle tre repositoryene fra Program.cs
    public AllocationController(
        IAllocationRepository allocationRepo,
        IResourceRepository resourceRepo,
        INeedRepository needRepo)
    {
        _allocationRepo = allocationRepo;
        _resourceRepo = resourceRepo;
        _needRepo = needRepo;
    }

    // GET: /Allocation/Create?resourceIds=3&resourceIds=7
    // Viser de valgte ressursene og lar brukeren velge et behov.
    public async Task<IActionResult> Create(List<int> resourceIds)
    {
        // Ingen ressurser valgt: tilbake til listen med en melding
        if (resourceIds.Count == 0)
        {
            TempData["Message"] = "Velg minst én ressurs før du allokerer.";
            return RedirectToAction("Index", "Resource");
        }

        var model = new AllocateViewModel { ResourceIds = resourceIds };
        await FillAsync(model);
        return View(model);
    }

    // POST: /Allocation/Create
    // Lagrer allokeringene og setter ressursene som utilgjengelige.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AllocateViewModel model)
    {
        if (model.ResourceIds.Count == 0)
        {
            return RedirectToAction("Index", "Resource");
        }

        // Ugyldig skjema (f.eks. ikke valgt behov): vis siden på nytt med feilmelding
        if (!ModelState.IsValid)
        {
            await FillAsync(model);
            return View(model);
        }

        // Sjekker at behovet faktisk finnes
        var need = await _needRepo.GetByIdAsync(model.NeedId!.Value);
        if (need == null)
        {
            return NotFound();
        }

        var count = await _allocationRepo.AllocateAsync(need.Id, model.ResourceIds, model.Note);

        TempData["Message"] = count > 0
            ? $"{count} ressurs(er) allokert til behovet."
            : "Ingen av ressursene var tilgjengelige lenger. Ingenting ble allokert.";

        // Sender brukeren til behovssiden, der de allokerte ressursene vises
        return RedirectToAction("Confirmation", "Need", new { id = need.Id });
    }
    
    [HttpGet]
    public async Task<IActionResult> FromNeed(int needId, string? q = null, string? category = null)
    {
        var model = new NeedAllocationViewModel { NeedId = needId, Q = q, Category = category };
        if (!await FillSuggestionsAsync(model)) return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FromNeed(NeedAllocationViewModel model)
    {
        if (!await FillSuggestionsAsync(model)) return NotFound();
        var selected = model.ResourceIds.Distinct().ToList();
        if (selected.Count == 0)
            ModelState.AddModelError(nameof(model.ResourceIds), "Velg minst én foreslått ressurs.");

        // Beregn på nytt: skjulte ID-er kan endres, og tilgjengeligheten kan ha endret seg.
        if (selected.Any(id => !model.Suggestions.Any(r => r.Id == id)))
            ModelState.AddModelError(nameof(model.ResourceIds), "Forslagene har endret seg. Velg blant de tilgjengelige ressursene nedenfor.");

        if (!ModelState.IsValid) return View(model);

        var count = await _allocationRepo.AllocateAsync(model.NeedId, selected, model.Note);
        TempData["Message"] = count > 0
            ? $"{count} ressurs(er) allokert til behovet."
            : "Ingen av ressursene var tilgjengelige lenger. Ingenting ble allokert.";
        return RedirectToAction("Confirmation", "Need", new { id = model.NeedId });
    }

    private async Task<bool> FillSuggestionsAsync(NeedAllocationViewModel model)
    {
        var need = await _needRepo.GetByIdAsync(model.NeedId);
        if (need is null) return false;
        model.NeedTitle = need.Type;
        model.Location = need.Location;
        var nearby = NearbyResourceSuggestions.Find(need, await _resourceRepo.GetAllAsync());
        model.CategoryOptions = nearby
            .Select(r => r.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c!)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        // Filtreringen gjelder bare tilgjengelige ressurser innenfor 5 km.
        var query = model.Q?.Trim();
        model.Suggestions = nearby
            .Where(r => string.IsNullOrWhiteSpace(model.Category) || r.Category == model.Category)
            .Where(r => string.IsNullOrWhiteSpace(query)
                || r.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                || r.Location.Contains(query, StringComparison.OrdinalIgnoreCase)
                || (r.Category?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
            .Take(3)
            .ToList();
        return true;
    }

    // POST: /Allocation/Remove/5
    // Fjerner en ressurs fra et behov, og sender brukeren tilbake til siden de kom fra.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id, string? returnTo)
    {
        var needId = await _allocationRepo.RemoveAsync(id);

        if (needId == null)
        {
            return NotFound();
        }

        TempData["Message"] = "Ressursen er fjernet fra behovet og er tilgjengelig igjen.";

        // Bare to lovlige sider å gå tilbake til. Alt annet blir behovssiden.
        var action = returnTo == "Edit" ? "Edit" : "Confirmation";
        return RedirectToAction(action, "Need", new { id = needId });
    }

    // Fyller visningsdataene: de valgte ressursene og listen over behov.
    // Brukes både første gang siden vises og når den vises på nytt med feil.
    private async Task FillAsync(AllocateViewModel model)
    {
        var resources = await _resourceRepo.GetAllAsync();
        model.Resources = resources
            .Where(r => model.ResourceIds.Contains(r.Id) && r.Status == "Tilgjengelig")
            .Select(ToModel)
            .ToList();

        var needs = await _needRepo.GetAllAsync();
        model.NeedOptions = needs
            .OrderBy(n => n.Deadline)
            .Select(n => new SelectListItem
            {
                Value = n.Id.ToString(),
                Text = $"{n.Type} – {n.Location} (frist {n.Deadline:dd.MM.yyyy})"
            })
            .ToList();
    }

    // Gjør entityen om til modellen viewet bruker.
    // Bare feltene allokeringssiden viser.
    private static ResourceModel ToModel(ResourceEntity entity)
    {
        return new ResourceModel
        {
            Id = entity.Id,
            Type = entity.Type,
            Description = entity.Description,
            Area = entity.Name,
            Status = entity.Status,
            CategoryName = entity.Category?.Name
        };
    }
}