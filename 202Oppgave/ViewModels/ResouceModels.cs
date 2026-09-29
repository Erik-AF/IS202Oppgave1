using System;
using System.ComponentModel.DataAnnotations;

namespace _202Oppgave.ViewModels;

/*
 * ResourceViewModel – data mellom registreringsskjemaet og ResourceController.
 *
 * Speiler feltene i Resource.cs, men er det brukeren faktisk sender inn.
 * Display-attributtene styrer de norske labelene i Create.cshtml, siden
 * asp-for henter navnet derfra automatisk.
 */
public class ResourceViewModel
{
    [Required(ErrorMessage = "Velg type ressurs")]
    [Display(Name = "Type ressurs")]
    public string Type { get; set; } = string.Empty;

    [Display(Name = "Beskrivelse")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Oppgi sted")]
    [Display(Name = "Sted")]
    public string Area { get; set; } = string.Empty;

    // Fylles ut av kartet, ikke skrevet inn manuelt av bruker.
    [Display(Name = "Breddegrad")]
    public double? Latitude { get; set; }

    [Display(Name = "Lengdegrad")]
    public double? Longitude { get; set; }

    [Display(Name = "Tilgjengelig fra")]
    public DateTime? AvailableFrom { get; set; }

    [Display(Name = "Tilgjengelig til")]
    public DateTime? AvailableTo { get; set; }

    [Required(ErrorMessage = "Oppgi kontaktperson")]
    [Display(Name = "Kontaktperson")]
    public string ContactName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Oppgi telefonnummer")]
    [Display(Name = "Telefon")]
    public string ContactPhone { get; set; } = string.Empty;

    [Display(Name = "E-post")]
    public string? ContactEmail { get; set; }

    // Standardverdi når skjemaet åpnes tomt.
    [Display(Name = "Status")]
    public string Status { get; set; } = "Tilgjengelig";

    [Display(Name = "Eier")]
    public string? OwnerName { get; set; }
}