using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace _202Oppgave.ViewModels.Account;
/// <summary>Validerer innloggingsdata og bevarer ønsket returadresse.</summary>
public sealed class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}