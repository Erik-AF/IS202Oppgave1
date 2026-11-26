using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace _202Oppgave.ViewModels.Account;

public class EnableAuthenticatorViewModel
{
    [Required(ErrorMessage = "Du må skrive inn sikkerhetskoden.")]
    [RegularExpression(
        @"^[0-9]{6}$",
        ErrorMessage = "Koden må inneholde seks sifre.")]
    [Display(Name = "Sikkerhetskode")]
    public string Code { get; set; } = string.Empty;

    [BindNever]
    public string SharedKey { get; set; } = string.Empty;

    [BindNever]
    public string QrCodeImage { get; set; } = string.Empty;
    [Required]
    public string SetupToken { get; set; } = string.Empty;
}