using System.Security.Claims;
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
 * Leverandører kan tilby egne ressurser; behovets eier godkjenner tilbud.
 */
[Authorize(Roles = RoleNames.PublicActor + "," + RoleNames.Admin + "," + RoleNames.ResourceProvider)]
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

    private bool IsProvider => User.IsInRole(RoleNames.ResourceProvider);
    private bool CanManage(_202Oppgave.Models.Entities.Need need) =>
        User.IsInRole(RoleNames.Admin)
        || (User.IsInRole(RoleNames.PublicActor)
            && !string.IsNullOrEmpty(need.UserId)
            && need.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

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

        if (IsProvider && !await OwnsResourcesAsync(resourceIds)) return Forbid();
        var model = new AllocateViewModel { ResourceIds = resourceIds };
        await FillAsync(model);
        return View(model);
    }

    // POST: /Allocation/Create
    // Leverandører sender tilbud; offentlige aktører beholder direkte allokering.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AllocateViewModel model)
    {
        if (model.ResourceIds.Count == 0)
        {
            return RedirectToAction("Index", "Resource");
        }

        if (IsProvider && !await OwnsResourcesAsync(model.ResourceIds)) return Forbid();

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

        if (IsProvider)
        {
            if (string.IsNullOrWhiteSpace(need.UserId))
            {
                ModelState.AddModelError(nameof(model.NeedId), "Dette behovet mangler en oppretter som kan godkjenne.");
                await FillAsync(model);
                return View(model);
            }
            var offered = await _allocationRepo.OfferAsync(need.Id, model.ResourceIds,
                User.FindFirstValue(ClaimTypes.NameIdentifier)!, model.Note);
            TempData["Message"] = offered > 0
                ? $"{offered} ressurs(er) sendt til godkjenning."
                : "Ressursen er ikke tilgjengelig lenger, eller er allerede tilbudt.";
            return RedirectToAction("Confirmation", "Need", new { id = need.Id });
        }
        var count = await _allocationRepo.AllocateAsync(need.Id, model.ResourceIds, model.Note);

        TempData["Message"] = count > 0
            ? $"{count} ressurs(er) allokert til behovet."
            : "Ingen av ressursene var tilgjengelige lenger. Ingenting ble allokert.";

        // Sender brukeren til behovssiden, der de allokerte ressursene vises
        return RedirectToAction("Confirmation", "Need", new { id = need.Id });
    }
    
    [Authorize(Roles = RoleNames.PublicActor + "," + RoleNames.Admin)]
    [HttpGet]
    public async Task<IActionResult> FromNeed(int needId)
    {
        var model = new NeedAllocationViewModel { NeedId = needId };
        if (!await FillSuggestionsAsync(model)) return NotFound();
        return View(model);
    }

    [Authorize(Roles = RoleNames.PublicActor + "," + RoleNames.Admin)]
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
        model.Suggestions = NearbyResourceSuggestions.Find(need, await _resourceRepo.GetAllAsync());
        return true;
    }

    // POST: /Allocation/Remove/5
    // Fjerner en ressurs fra et behov, og sender brukeren tilbake til siden de kom fra.
    [Authorize(Roles = RoleNames.PublicActor + "," + RoleNames.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id, string? returnTo)
    {
        var allocation = await _allocationRepo.GetByIdAsync(id);
        if (allocation == null) return NotFound();
        if (!CanManage(allocation.Need)) return Forbid();
        var needId = await _allocationRepo.RemoveAsync(id);

        if (needId == null)
        {
            return NotFound();
        }

        TempData["Message"] = "Ressursen er fjernet fra behovet.";

        // Bare to lovlige sider å gå tilbake til. Alt annet blir behovssiden.
        var action = returnTo == "Edit" ? "Edit" : "Confirmation";
        return RedirectToAction(action, "Need", new { id = needId });
    }

    [Authorize(Roles = RoleNames.PublicActor + "," + RoleNames.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, string? returnTo)
    {
        var allocation = await _allocationRepo.GetByIdAsync(id);
        if (allocation == null) return NotFound();
        if (!CanManage(allocation.Need)) return Forbid();
        var approved = await _allocationRepo.ApproveAsync(id);
        TempData["Message"] = approved
            ? "Tilbudet er godkjent og ressursen er allokert."
            : "Tilbudet kunne ikke godkjennes. Det kan allerede være behandlet.";
        return RedirectToAction(returnTo == "Edit" ? "Edit" : "Confirmation",
            "Need", new { id = allocation.NeedId });
    }

    // Ressurseierskap kontrolleres også ved POST; skjulte ID-er kan endres.
    private async Task<bool> OwnsResourcesAsync(List<int> ids)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return false;
        var resources = await _resourceRepo.GetAllAsync();
        return ids.All(id => resources.Any(r => r.Id == id && r.UserId == userId));
    }

    // Fyller visningsdataene: de valgte ressursene og listen over behov.
    // Brukes både første gang siden vises og når den vises på nytt med feil.
    private async Task FillAsync(AllocateViewModel model)
    {
        model.IsOffer = IsProvider;
        var resources = await _resourceRepo.GetAllAsync();
        model.Resources = resources
            .Where(r => model.ResourceIds.Contains(r.Id) && r.Status == "Tilgjengelig"
                && (!IsProvider || r.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)))
            .Select(ToModel)
            .ToList();

        var needs = await _needRepo.GetAllAsync();
        model.NeedOptions = needs
            .Where(n => !IsProvider || !string.IsNullOrWhiteSpace(n.UserId))
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