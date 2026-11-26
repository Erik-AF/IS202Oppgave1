using Microsoft.AspNetCore.Mvc;
using _202Oppgave.Models;

namespace _202Oppgave.Controllers;

/// Viser informasjonssider og en oppsummering av kontaktskjemaet. Meldingen sendes ikke som e-post.
public class InformationController : Controller
{

    public IActionResult Om()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Kontakt()
    {
        return View();
    }

    /// Validerer skjemaet og viser en oppsummering. Meldingen lagres ikke og sendes ikke som e-post.
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

    public IActionResult Hjelp()
    {
        return View();
    }
}