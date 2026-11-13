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

    // Adresseforslag fra Kartverkets åpne adresse-API.
    private static readonly System.Net.Http.HttpClient AddressClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    [HttpGet]
    public async Task<IActionResult> ReverseGeocode(
        [FromQuery] double? lat, [FromQuery] double? lon)
    {
        if (!ModelState.IsValid || lat is null || lon is null ||
            !double.IsFinite(lat.Value) || !double.IsFinite(lon.Value) ||
            lat.Value < -90 || lat.Value > 90 ||
            lon.Value < -180 || lon.Value > 180)
        {
            return BadRequest(new { message = "Ugyldige koordinater." });
        }

        var latitude = lat.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var longitude = lon.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var url = "https://api.kartverket.no/adresser/v1/punktsok" +
            $"?lat={latitude}&lon={longitude}&koordsys=4326" +
            "&radius=100&treffPerSide=1";

        try
        {
            using var response = await AddressClient.GetAsync(url, HttpContext.RequestAborted);
            if (!response.IsSuccessStatusCode)
                return StatusCode(503, new { message = "Adresseoppslaget er utilgjengelig." });

            using var json = System.Text.Json.JsonDocument.Parse(
                await response.Content.ReadAsStringAsync(HttpContext.RequestAborted));

            if (!json.RootElement.TryGetProperty("adresser", out var addresses) ||
                addresses.ValueKind != System.Text.Json.JsonValueKind.Array ||
                addresses.GetArrayLength() == 0)
            {
                return Json(new { address = (string?)null });
            }

            var first = addresses[0];
            string ReadText(string name) =>
                first.TryGetProperty(name, out var value) &&
                value.ValueKind == System.Text.Json.JsonValueKind.String
                    ? value.GetString() ?? "" : "";

            var street = ReadText("adressetekst");
            var postal = (ReadText("postnummer") + " " + ReadText("poststed")).Trim();
            var address = string.Join(", ",
                new[] { street, postal }.Where(part => !string.IsNullOrWhiteSpace(part)));

            return Json(new { address = string.IsNullOrWhiteSpace(address) ? null : address });
        }
        catch (Exception error) when (
            error is System.Net.Http.HttpRequestException ||
            error is OperationCanceledException ||
            error is System.Text.Json.JsonException)
        {
            return StatusCode(503, new { message = "Kunne ikke hente adresse. Skriv stedet manuelt." });
        }
    }
}

