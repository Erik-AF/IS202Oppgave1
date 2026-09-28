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
    public IActionResult Kontakt()
    {
        return View();
    }

    // Metode for innsending av kontaktskjemaet
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Kontakt(KontaktFormModel model)
    {
        // Sjekker at alle feltene er fylt ut
        if (string.IsNullOrWhiteSpace(model.Navn) ||
            string.IsNullOrWhiteSpace(model.Epost) ||
            string.IsNullOrWhiteSpace(model.Emne) ||
            string.IsNullOrWhiteSpace(model.Melding))
        {
            ViewBag.Feil = "Du må fylle ut alle feltene.";
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