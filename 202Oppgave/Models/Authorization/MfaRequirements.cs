using System.Security.Claims;

namespace _202Oppgave.Models.Authorization;

public static class MfaRequirements
{
    public static bool IsRequired(ClaimsPrincipal user)
    {
        return user.IsInRole(RoleNames.Admin)
            || user.IsInRole(RoleNames.PublicActor);
    }
}