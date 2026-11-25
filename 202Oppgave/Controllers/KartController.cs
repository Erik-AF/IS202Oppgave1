using Microsoft.AspNetCore.Mvc;
using _202Oppgave.ViewModels;

namespace _202Oppgave.Controllers;

/// <summary>Viser kartoversikten, håndterer separate kartpunkter i minnet og slår opp adresser hos Kartverket.</summary>
public class KartController : Controller
{

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

    /// <summary>Viser registrering av separate kartpunkter.</summary>
    [HttpGet]
    public IActionResult Index()
    {
        return View(new RegistrerPunktViewModel());
    }

    /// <summary>Legger kartpunktet i en minneliste som nullstilles ved omstart, og omdirigerer til oversikten.</summary>
    [HttpPost]
    public IActionResult Index(RegistrerPunktViewModel model)
    {
        if (model.Latitude == null || model.Longitude == null)
        {

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

        return RedirectToAction(nameof(Oversikt));
    }

    /// <summary>Viser kartoversikten som henter kartpunkter, behov og ressurser fra separate JSON-endepunkter.</summary>
    [HttpGet]
    public IActionResult Oversikt()
    {
        return View();
    }

    /// <summary>Returnerer punktene fra minnelisten; behov og ressurser i databasen har egne kartendepunkter.</summary>
    [HttpGet]
    public IActionResult Data()
    {
        return Json(_punkter);
    }

    private static readonly System.Net.Http.HttpClient AddressClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    /// <summary>Validerer koordinater og foreslår nærmeste adresse innenfor 100 meter fra Kartverket; feil gir mulighet for manuell innskriving.</summary>
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
            /// Leser et tekstfelt fra adresseoppslaget og bruker tom tekst når feltet mangler.
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

