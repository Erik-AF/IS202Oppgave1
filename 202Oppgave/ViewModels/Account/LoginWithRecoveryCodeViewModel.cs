using System.ComponentModel.DataAnnotations;

namespace _202Oppgave.ViewModels.Account;

public class LoginWithRecoveryCodeViewModel
{
    [Required(ErrorMessage = "Du må skrive inn en gjenopprettingskode.")]
    [StringLength(64)]
    [Display(Name = "Gjenopprettingskode")]
    public string Code { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}