using Microsoft.AspNetCore.Mvc;
using _202Oppgave.Models;

namespace _202Oppgave.Controllers;

/// <summary>Viser informasjonssider og en oppsummering av kontaktskjemaet; meldingen sendes ikke som e-post.</summary>
public class InformationController : Controller
{

    /// <summary>Viser informasjon om tjenesten.</summary>
    public IActionResult Om()
    {
        return View();
    }

    /// <summary>Viser kontaktskjemaet.</summary>
    [HttpGet]
    public IActionResult Kontakt()
    {
        return View();
    }

    /// <summary>Validerer skjemaet og viser en oppsummering; meldingen lagres ikke og sendes ikke som e-post.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Kontakt(KontaktFormModel model)
    {

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return View("KontaktBekreftelse", model);
    }

    /// <summary>Viser hjelpesiden.</summary>
    public IActionResult Hjelp()
    {
        return View();
    }
}