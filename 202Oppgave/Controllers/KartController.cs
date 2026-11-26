using Microsoft.AspNetCore.Mvc;
namespace _202Oppgave.Controllers;

/// Viser kartoversikten, håndterer separate kartpunkter i minnet og slår opp adresser hos Kartverket.
public class KartController : Controller
{
    [HttpGet]
    public IActionResult Oversikt()
    {
        return View();
    }
    private static readonly System.Net.Http.HttpClient AddressClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    /// Validerer koordinater og foreslår nærmeste adresse innenfor 100 meter fra Kartverket. Feil gir mulighet for manuell innskriving.
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

