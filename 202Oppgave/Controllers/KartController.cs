using Microsoft.AspNetCore.Mvc;
using _202Oppgave.ViewModels;

namespace _202Oppgave.Controllers;

public class KartController : Controller
{
    // Midlertidig eksempeldata i minnet, inntil ekte database (MariaDB)
    // er koblet på av Vetle/Sindre. Formen (KartPunktViewModel) er avtalt
    // som "kontrakten" med dem.
    private static readonly List<KartPunktViewModel> _punkter = new()
    {
        new KartPunktViewModel
        {
            Id = 1,
            Kategori = "behov",
            Type = "Drikkevann",
            Latitude = 58.1467,
            Longitude = 7.9956,
            Prioritet = "Akutt",
            Status = "Registrert",
            Kontaktpunkt = "Kristiansand kommune",
            Beskrivelse = "Behov for drikkevann i sentrum."
        }
    };
    // Viser registreringsskjemaet med kart
    [HttpGet]
    public IActionResult Index()
    {
        return View(new RegistrerPunktViewModel());
    }
    // Tar imot skjemaet med valgt kartpunkt og lagrer det.
    [HttpPost]
    public IActionResult Index(RegistrerPunktViewModel model)
    {
        if (model.Latitude == null || model.Longitude == null)
        {
            // Ingen punkt valgt på kartet - vis skjemaet på nytt med feilmelding
            ModelState.AddModelError("", "Du må velge et punkt på kartet.");
            return View(model);
        }
        _punkter.Add(new KartPunktViewModel
        {
            Id = _punkter.Count + 1,
            Kategori = model.Kategori,
            Type = model.Type,
            Latitude = model.Latitude.Value,
            Longitude = model.Longitude.Value,
            Prioritet = "Ikke satt",
            Status = "Registrert",
            Kontaktpunkt = "Ikke satt",
            Beskrivelse = model.Beskrivelse
        });
        // PRG-mønster (Post-Redirect-Get): redirect etter POST i stedet for
        // å returnere View() direkte, slik at brukeren ikke får "skjema sendt
        // på nytt"-advarsel hvis siden lastes om.
        return RedirectToAction(nameof(Oversikt));
    }
    // Viser oversiktskartet med alle registrerte punkter
    [HttpGet]
    public IActionResult Oversikt()
    {
        return View();
    }
    
    // JSON-endepunkt som oversiktskartet henter data fra via JavaScript (fetch)
    [HttpGet]
    public IActionResult Data()
    {
        return Json(_punkter);
    }
}