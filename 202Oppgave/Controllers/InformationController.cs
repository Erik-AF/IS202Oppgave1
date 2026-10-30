using Microsoft.AspNetCore.Mvc;
using _202Oppgave.Models;

namespace _202Oppgave.Controllers;

public class InformationController : Controller
{
    // Metode for Om-siden
    public IActionResult Om()
    {
        return View();
    }

    // Metode for Kontakt-siden
    [HttpGet]
    public IActionResult Kontakt()
    {
        return View();
    }

    // Metode for innsending av kontaktskjemaet
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Kontakt(KontaktFormModel model)
    {
        // Sjekker om feltene oppfyller valideringskravene
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Viser bekreftelsessiden med dataene fra skjemaet
        return View("KontaktBekreftelse", model);
    }

    // Metode for Hjelp-siden
    public IActionResult Hjelp()
    {
        return View();
    }
}