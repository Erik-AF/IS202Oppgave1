using System.ComponentModel.DataAnnotations;

namespace _202Oppgave.ViewModels.Account;

public class LoginWith2faViewModel
{
    [Required(ErrorMessage = "Du må skrive inn sikkerhetskoden.")]
    [RegularExpression(
        @"^[0-9]{6}$",
        ErrorMessage = "Koden må inneholde seks sifre.")]
    [Display(Name = "Sikkerhetskode")]
    public string Code { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}