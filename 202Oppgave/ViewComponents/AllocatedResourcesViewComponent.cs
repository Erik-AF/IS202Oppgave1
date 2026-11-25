using _202Oppgave.DataAccess.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace _202Oppgave.ViewComponents;

/// Laster allokeringer og tilbud for visning under ett behov.
public class AllocatedResourcesViewComponent : ViewComponent
{
    private readonly IAllocationRepository _allocationRepo;

    public AllocatedResourcesViewComponent(IAllocationRepository allocationRepo)
    {
        _allocationRepo = allocationRepo;
    }

    /// Henter behovets allokeringer med ressursdata og bevarer hvilken behovsside handlingene skal returnere til.
    public async Task<IViewComponentResult> InvokeAsync(int needId, string returnTo = "Confirmation")
    {
        var allocations = await _allocationRepo.GetForNeedAsync(needId);
        ViewData["ReturnTo"] = returnTo;
        return View(allocations);
    }
}