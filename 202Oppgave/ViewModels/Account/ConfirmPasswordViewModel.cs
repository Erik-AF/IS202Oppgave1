using System.ComponentModel.DataAnnotations;

namespace _202Oppgave.ViewModels.Account;

public class ConfirmPasswordViewModel
{
    [Required(ErrorMessage = "Du må skrive inn passordet ditt.")]
    [DataType(DataType.Password)]
    [Display(Name = "Gjeldende passord")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Sikkerhetskode")]
    public string? Code { get; set; }
}