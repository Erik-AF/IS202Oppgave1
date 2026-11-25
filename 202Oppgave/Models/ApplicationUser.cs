using Microsoft.AspNetCore.Identity;

namespace _202Oppgave.Models.Entities;

/// Utvider Identity-brukeren med fullt navn for profil og kontaktinformasjon.
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}