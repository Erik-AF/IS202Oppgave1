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

/// Håndterer direkte allokering og tilbud som må godkjennes av behovets eier eller administrator.
[Authorize(Roles = RoleNames.PublicActor + "," + RoleNames.Admin + "," + RoleNames.ResourceProvider)]
public class AllocationController : Controller
{
    private readonly IAllocationRepository _allocationRepo;
    private readonly IResourceRepository _resourceRepo;
    private readonly INeedRepository _needRepo;

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
    /// Tillater administrator eller offentlig aktør som eier behovet å behandle tilbud.
    private bool CanManage(_202Oppgave.Models.Entities.Need need) =>
        User.IsInRole(RoleNames.Admin)
        || (User.IsInRole(RoleNames.PublicActor)
            && !string.IsNullOrEmpty(need.UserId)
            && need.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

    /// Viser leverandørens tilgjengelige ressurser for det valgte behovet; innsending går direkte til godkjenning.
    [Authorize(Roles = RoleNames.ResourceProvider)]
    [HttpGet]
    public async Task<IActionResult> OfferToNeed(int needId)
    {
        if (!IsProvider) return Forbid();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();
        var need = await _needRepo.GetByIdAsync(needId);
        if (need == null) return NotFound();
        var resources = await _resourceRepo.GetByUserIdAsync(userId);
        var model = new AllocateViewModel
        {
            NeedId = needId,
            IsOffer = true,
            Resources = resources.Where(r => r.Status == "Tilgjengelig").Select(ToModel).ToList(),
            NeedOptions = new() { new SelectListItem { Value = needId.ToString(), Text = need.Type } }
        };
        if (string.IsNullOrWhiteSpace(need.UserId))
        {
            ModelState.AddModelError("", "Dette behovet mangler en oppretter som kan godkjenne.");
            model.Resources.Clear();
        }
        return View(model);
    }

    /// Viser valgte ressurser og behovsvalg etter eierskapskontroll for leverandører.
    public async Task<IActionResult> Create(List<int> resourceIds, int? needId = null)
    {

        if (resourceIds.Count == 0)
        {
            TempData["Message"] = "Velg minst én ressurs før du allokerer.";
            return RedirectToAction("Index", "Resource");
        }

        if (IsProvider && !await OwnsResourcesAsync(resourceIds)) return Forbid();
        if (needId.HasValue && await _needRepo.GetByIdAsync(needId.Value) == null) return NotFound();
        var model = new AllocateViewModel { ResourceIds = resourceIds, NeedId = needId };
        await FillAsync(model);
        return View(model);
    }

    /// Sender leverandørens egne ressurser som tilbud; andre tillatte roller allokerer direkte.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AllocateViewModel model)
    {
        if (model.ResourceIds.Count == 0)
        {
            return RedirectToAction("Index", "Resource");
        }

        if (IsProvider && !await OwnsResourcesAsync(model.ResourceIds)) return Forbid();

        if (!ModelState.IsValid)
        {
            await FillAsync(model);
            return View(model);
        }

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

        return RedirectToAction("Confirmation", "Need", new { id = need.Id });
    }

    /// Viser inntil tre filtrerte ressursforslag for behovet.
    [Authorize(Roles = RoleNames.PublicActor + "," + RoleNames.Admin)]
    [HttpGet]
    public async Task<IActionResult> FromNeed(int needId, string? q = null, string? category = null)
    {
        var model = new NeedAllocationViewModel { NeedId = needId, Q = q, Category = category };
        if (!await FillSuggestionsAsync(model)) return NotFound();
        return View(model);
    }

    /// Beregner forslag på nytt før lagring, fordi innsendte ID-er og tilgjengelighet kan ha endret seg.
    [Authorize(Roles = RoleNames.PublicActor + "," + RoleNames.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FromNeed(NeedAllocationViewModel model)
    {
        if (!await FillSuggestionsAsync(model)) return NotFound();
        var selected = model.ResourceIds.Distinct().ToList();
        if (selected.Count == 0)
            ModelState.AddModelError(nameof(model.ResourceIds), "Velg minst én foreslått ressurs.");

        if (selected.Any(id => !model.Suggestions.Any(r => r.Id == id)))
            ModelState.AddModelError(nameof(model.ResourceIds), "Forslagene har endret seg. Velg blant de tilgjengelige ressursene nedenfor.");

        if (!ModelState.IsValid) return View(model);

        var count = await _allocationRepo.AllocateAsync(model.NeedId, selected, model.Note);
        TempData["Message"] = count > 0
            ? $"{count} ressurs(er) allokert til behovet."
            : "Ingen av ressursene var tilgjengelige lenger. Ingenting ble allokert.";
        return RedirectToAction("Confirmation", "Need", new { id = model.NeedId });
    }

    /// Filtrerer tilgjengelige ressurser innenfor fem kilometer og beholder høyst tre av de nærmeste treffene.
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

    /// Kontrollerer behandlingsrett og fjerner allokeringen før retur til redigering eller behovsdetaljer.
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

        var action = returnTo == "Edit" ? "Edit" : "Confirmation";
        return RedirectToAction(action, "Need", new { id = needId });
    }

    /// Kontrollerer behandlingsrett og forsøker å bekrefte det ventende tilbudet.
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

    /// Kontrollerer alle innsendte ressurs-ID-er mot innlogget bruker; skjulte skjemafelt er ikke bevis på eierskap.
    private async Task<bool> OwnsResourcesAsync(List<int> ids)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return false;
        var resources = await _resourceRepo.GetAllAsync();
        return ids.All(id => resources.Any(r => r.Id == id && r.UserId == userId));
    }

    /// Gjenoppbygger ressurs- og behovsvalg, også etter valideringsfeil; rollen bestemmer tilbudsmodus.
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