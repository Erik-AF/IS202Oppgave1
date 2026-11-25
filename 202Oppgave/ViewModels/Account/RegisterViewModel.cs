using System.ComponentModel.DataAnnotations;

namespace _202Oppgave.ViewModels.Account;

public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "Du må oppgi fullt navn.")]
    [StringLength(150, MinimumLength = 2)]
    [Display(Name = "Fullt navn")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du må oppgi telefonnummer.")]
    [Phone(ErrorMessage = "Oppgi et gyldig telefonnummer.")]
    [Display(Name = "Telefonnummer")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password),
        ErrorMessage = "The passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Du må velge en brukertype.")]
    public string Role { get; set; } = string.Empty;
}