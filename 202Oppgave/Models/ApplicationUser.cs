using Microsoft.AspNetCore.Identity;

namespace _202Oppgave.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}