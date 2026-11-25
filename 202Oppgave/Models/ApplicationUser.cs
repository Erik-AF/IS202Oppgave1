using Microsoft.AspNetCore.Identity;

namespace _202Oppgave.Models.Entities;

/// <summary>Utvider Identity-brukeren med fullt navn for profil og kontaktinformasjon.</summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}