using Microsoft.AspNetCore.Mvc;

namespace _202Oppgave.Models.Authorization
{
    /// <summary>Samler rollenavnene som brukes ved registrering og tilgangskontroll.</summary>
    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string ResourceProvider = "ResourceProvider";
        public const string PublicActor = "PublicActor";
    }
}
