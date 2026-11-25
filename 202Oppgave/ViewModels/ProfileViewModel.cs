using System.ComponentModel.DataAnnotations;

namespace _202Oppgave.ViewModels;

/// <summary>Validerer navn, telefon og e-post ved profilendring.</summary>
public class ProfileViewModel
{
    [Required(ErrorMessage = "Du må oppgi fullt navn.")]
    [StringLength(150, MinimumLength = 2)]
    [Display(Name = "Fullt navn")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du må oppgi telefonnummer.")]
    [Phone(ErrorMessage = "Oppgi et gyldig telefonnummer.")]
    [Display(Name = "Telefonnummer")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du må oppgi e-postadresse.")]
    [EmailAddress(ErrorMessage = "Oppgi en gyldig e-postadresse.")]
    [Display(Name = "E-postadresse")]
    public string Email { get; set; } = string.Empty;
}