using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace _202Oppgave.ViewModels;

/// <summary>Validerer ressursens skjemadata, tilgjengelighetsperiode og eventuelle bildefil.</summary>
public class ResourceViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Skriv en tittel")]
    [StringLength(100, ErrorMessage = "Maks 100 tegn")]
    [Display(Name = "Tittel")]
    public string Type { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Maks 500 tegn")]
    [Display(Name = "Beskrivelse")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Oppgi sted")]
    [StringLength(100, ErrorMessage = "Maks 100 tegn")]
    [Display(Name = "Sted")]
    public string Area { get; set; } = string.Empty;

    [Required(ErrorMessage = "Velg en posisjon i kartet")]
    [Range(-90, 90, ErrorMessage = "Breddegrad må være mellom -90 og 90")]
    [Display(Name = "Breddegrad")]
    public double? Latitude { get; set; }

    [Required(ErrorMessage = "Velg en posisjon i kartet")]
    [Range(-180, 180, ErrorMessage = "Lengdegrad må være mellom -180 og 180")]
    [Display(Name = "Lengdegrad")]
    public double? Longitude { get; set; }

    public string? GeometryJson { get; set; }

    [Display(Name = "Tilgjengelig fra")]
    public DateTime? AvailableFrom { get; set; }

    [Display(Name = "Tilgjengelig til")]
    public DateTime? AvailableTo { get; set; }

    [StringLength(100, ErrorMessage = "Maks 100 tegn")]
    [Display(Name = "Kontaktperson")]
    public string ContactName { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Maks 20 tegn")]
    [Display(Name = "Telefon")]
    public string ContactPhone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Ugyldig e-postadresse")]
    [StringLength(150, ErrorMessage = "Maks 150 tegn")]
    [Display(Name = "E-post")]
    public string? ContactEmail { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; } = "Tilgjengelig";

    [StringLength(100, ErrorMessage = "Maks 100 tegn")]
    [Display(Name = "Eier")]
    public string? OwnerName { get; set; }

    [Display(Name = "Bilde")]
    public IFormFile? Image { get; set; }

    public string? ImagePath { get; set; }

    public static readonly string[] AllowedImageTypes = { ".jpg", ".jpeg", ".png", ".webp" };
    public const long MaxImageBytes = 5 * 1024 * 1024;

    [Required(ErrorMessage = "Velg kategori")]
    [Display(Name = "Kategori")]
    public int? CategoryId { get; set; }

    public List<SelectListItem> CategoryOptions { get; set; } = new();

    /// <summary>Validerer tilgjengelighetsperioden og bildefilens type og størrelse på tvers av enkeltfeltene.</summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (AvailableFrom.HasValue && AvailableTo.HasValue && AvailableTo < AvailableFrom)
        {
            yield return new ValidationResult(
                "Tilgjengelig til kan ikke være før tilgjengelig fra",
                new[] { nameof(AvailableTo) });
        }

        if (Image != null)
        {
            var extension = Path.GetExtension(Image.FileName).ToLowerInvariant();

            if (!AllowedImageTypes.Contains(extension))
            {
                yield return new ValidationResult(
                    "Bildet må være JPG, PNG eller WEBP",
                    new[] { nameof(Image) });
            }

            if (Image.Length > MaxImageBytes)
            {
                yield return new ValidationResult(
                    "Bildet kan ikke være større enn 5 MB",
                    new[] { nameof(Image) });
            }
        }
    }

}

