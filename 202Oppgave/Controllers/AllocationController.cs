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