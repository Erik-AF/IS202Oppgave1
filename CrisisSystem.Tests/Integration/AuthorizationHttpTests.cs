using System.Net;
using _202Oppgave.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace CrisisSystem.Tests.Integration;

public class AuthorizationHttpTests
{
    /// Testverten bruker ekte MVC-ruting og autorisasjonsfiltre, men starter ikke Program.cs eller databasen.
    [Theory]
    [InlineData("/ResourceProvider", "GET")]
    [InlineData("/PublicActor", "GET")]
    [InlineData("/Need/Create", "GET")]
    [InlineData("/Resource/Create", "GET")]
    [InlineData("/Account/Logout", "POST")]
    public async Task ProtectedEndpoints_AnonymousRequestRedirectsToLogin(string path, string method)
    {
        using var host = await new HostBuilder().ConfigureWebHost(web => web.UseTestServer().ConfigureServices(services => {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o => o.LoginPath = "/Account/Login");
            services.AddAuthorization();
            services.AddControllersWithViews().AddApplicationPart(typeof(AccountController).Assembly);
        }).Configure(app => {
            app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}"));
        })).StartAsync();
        using var response = await host.GetTestClient().SendAsync(new HttpRequestMessage(new HttpMethod(method), path));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location!.ToString());
    }
}
