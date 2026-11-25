using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace _202Oppgave.ViewModels.Account
{
    /// <summary>Validerer e-postadressen ved forespørsel om nytt passord.</summary>
    public sealed class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
