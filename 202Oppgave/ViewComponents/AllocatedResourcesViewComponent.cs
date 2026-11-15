using _202Oppgave.DataAccess.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace _202Oppgave.ViewComponents;

/*
 * AllocatedResourcesViewComponent – viser ressursene som er allokert til ett behov.
 * Brukes på behovssiden (Need/Confirmation) og på Rediger behov (Need/Edit).
 * Henter dataene selv, så NeedController trenger ingen endringer.
 */
public class AllocatedResourcesViewComponent : ViewComponent
{
    private readonly IAllocationRepository _allocationRepo;

    public AllocatedResourcesViewComponent(IAllocationRepository allocationRepo)
    {
        _allocationRepo = allocationRepo;
    }

    // Kalles fra viewet med: @await Component.InvokeAsync("AllocatedResources", new { needId = ... })
    // returnTo sier hvilken side brukeren skal tilbake til etter "Fjern":
    // "Confirmation" (behovssiden) eller "Edit" (Rediger behov).
    public async Task<IViewComponentResult> InvokeAsync(int needId, string returnTo = "Confirmation")
    {
        var allocations = await _allocationRepo.GetForNeedAsync(needId);
        ViewData["ReturnTo"] = returnTo;
        return View(allocations);
    }
}