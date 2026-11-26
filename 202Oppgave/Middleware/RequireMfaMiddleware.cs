using _202Oppgave.Models.Authorization;
using _202Oppgave.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace _202Oppgave.Middleware;

public class RequireMfaMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync( HttpContext context,UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager)
    {
        if (context.User.Identity?.IsAuthenticated != true ||
            !MfaRequirements.IsRequired(context.User))
        {
            await next(context);
            return;
        }

        var controller = context.Request.RouteValues["controller"]?.ToString();

        var action = context.Request.RouteValues["action"]?.ToString();

        var user = await userManager
            .GetUserAsync(context.User);

        if (user is null)
        {
            await signInManager.SignOutAsync();

            context.Response.Redirect("/Account/Login");
            return;
        }

        var enabled = await userManager
            .GetTwoFactorEnabledAsync(user);

        var logoutRequest =
            controller == "Account" &&
            action == "Logout";

        var setupRequest =
            controller == "Security" &&
            (action == "Index" ||
             action == "StartAuthenticator" ||
             action == "EnableAuthenticator");

        if (!enabled)
        {
            if (setupRequest || logoutRequest)
            {
                await next(context);
                return;
            }

            if (!HttpMethods.IsGet(context.Request.Method))
            {
                context.Response.StatusCode =  StatusCodes.Status403Forbidden;

                return;
            }

            context.Response.Redirect("/Security/Index");
            return;
        }

        if (!context.User.HasClaim("amr", "mfa") &&
            !logoutRequest)
        {
            await signInManager.ForgetTwoFactorClientAsync();
            await signInManager.SignOutAsync();

            context.Response.Redirect("/Account/Login");
            return;
        }

        await next(context);
    }
}