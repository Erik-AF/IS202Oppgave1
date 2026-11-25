using System.Diagnostics;
using _202Oppgave.DataAccess.Repositories;
using Microsoft.AspNetCore.Mvc;
using _202Oppgave.Models;
using _202Oppgave.ViewModels;

namespace _202Oppgave.Controllers;

public class HomeController : Controller

{
private readonly INeedRepository _needRepository;
public HomeController(INeedRepository needRepository)
{
    _needRepository = needRepository;
}

/// Viser de tre registrerte behovene med lavest ID på forsiden.
public async Task<IActionResult> Index()
{

    var needs = await _needRepository.GetAllAsync();

    var oldestNeeds = needs
        .OrderBy(n => n.Id)
        .Take(3)
        .ToList();

    var model = new HomeViewModel()
    {
        ActiveNeedCount = needs.Count,

        ActiveNeedTypes = oldestNeeds
            .Select(n => n.Type)
            .ToList()
    };

    return View(model);
}
public IActionResult Privacy()
{
    return View();
}

/// Viser forespørsels-ID for feilsøking uten å mellomlagre feilsiden.
[ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public IActionResult Error()
{
    return View(new ErrorViewModel
    {
        RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
    });
}
}
