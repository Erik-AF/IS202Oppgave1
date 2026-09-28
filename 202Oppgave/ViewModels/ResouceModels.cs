using System;
using System.ComponentModel.DataAnnotations;

namespace _202Oppgave.ViewModels;

public class ResourceViewModel
{
    [Required(ErrorMessage = "Velg type ressurs")]
    public string Type { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Oppgi geografisk område")]
    public string Area { get; set; } = string.Empty;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }

    [Required(ErrorMessage = "Oppgi kontaktperson")]
    public string ContactName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Oppgi telefonnummer")]
    public string ContactPhone { get; set; } = string.Empty;

    public string? ContactEmail { get; set; }

    public string Status { get; set; } = "Tilgjengelig";

    public string? OwnerName { get; set; }
}