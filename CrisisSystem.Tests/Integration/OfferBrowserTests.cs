using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using _202Oppgave.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace CrisisSystem.Tests.Integration;

public sealed class BrowserFactAttribute : FactAttribute
{
    public BrowserFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("RUN_BROWSER_TESTS") != "1" || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEST_MARIADB_CONNECTION")))
            Skip = "Krever RUN_BROWSER_TESTS=1, TEST_MARIADB_CONNECTION og installert Chromium. Se docs/Testing.md.";
    }
}

[Trait("Category", "Browser")]
public class OfferBrowserTests
{
    [BrowserFact(Timeout = 180000)]
    public async Task ProviderOffers_OwnerApproves_BadgeDisappears_BothLogOut()
    {
        await using var database = await TestDatabase.Create();
        const string password = "Browser-Test123!";
        await using (var db = database.Open())
        {
            foreach (var entry in new[] { ("provider", "ResourceProvider"), ("owner", "PublicActor") })
            {
                var user = (await db.Users.FindAsync(entry.Item1))!;
                user.Email = user.UserName = entry.Item1 + "@browser.example.test";
                user.NormalizedEmail = user.NormalizedUserName = user.Email.ToUpperInvariant();
                user.EmailConfirmed = true; user.SecurityStamp = Guid.NewGuid().ToString();
                user.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(user, password);
                db.Roles.Add(new IdentityRole { Id = entry.Item2, Name = entry.Item2, NormalizedName = entry.Item2.ToUpperInvariant() });
                db.UserRoles.Add(new IdentityUserRole<string> { RoleId = entry.Item2, UserId = user.Id });
            }
            await db.SaveChangesAsync();
        }
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root != null && !Directory.Exists(Path.Combine(root.FullName, "202Oppgave"))) root = root.Parent;
        Assert.NotNull(root);
        var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port; listener.Stop();
        var url = $"http://127.0.0.1:{port}";
        var start = new ProcessStartInfo("dotnet") { WorkingDirectory = Path.Combine(root!.FullName, "202Oppgave"), UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var argument in new[] { "run", "--no-build", "--no-launch-profile", "--configuration", BuildConfiguration(), "--urls", url }) start.ArgumentList.Add(argument);
        start.Environment["ConnectionStrings__DefaultConnection"] = database.ConnectionString;
        start.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEndAsync(); var stderr = process.StandardError.ReadToEndAsync();
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var ready = false;
            for (var attempt = 0; attempt < 60 && !process.HasExited; attempt++)
            {
                try { ready = (await client.GetAsync(url + "/Account/Login")).IsSuccessStatusCode; } catch (HttpRequestException) { } catch (TaskCanceledException) { }
                if (ready) break;
                await Task.Delay(500);
            }
            Assert.True(ready, "Testappen startet ikke. Kontroller bygg og MariaDB-tilkobling.");
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            await using var provider = await browser.NewContextAsync(); await using var owner = await browser.NewContextAsync();
            var p = await provider.NewPageAsync(); var o = await owner.NewPageAsync();
            await Login(p, url, "provider@browser.example.test", password);
            await Login(o, url, "owner@browser.example.test", password);
            var evidence = Path.Combine(root.FullName, "TestResults", "browser");
            Directory.CreateDirectory(evidence);
            await owner.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true });
            await p.GotoAsync(url + "/Allocation/OfferToNeed/1");
            await p.Locator("#offer-resource").SelectOptionAsync("1");
            await p.GetByRole(AriaRole.Button, new() { Name = "Send til godkjenning", Exact = true }).ClickAsync();
            await Expect(p).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Need/Confirmation/1"));
            await o.GotoAsync(url + "/Need/Confirmation/1");
            await Expect(o.Locator(".profile-menu-toggle .approval-badge")).ToHaveTextAsync("1");
            await o.ScreenshotAsync(new() { Path = Path.Combine(evidence, "pending-approval.png"), FullPage = true });
            await o.GetByRole(AriaRole.Button, new() { Name = "Godkjenn", Exact = true }).ClickAsync();
            await Expect(o.Locator(".profile-menu-toggle .approval-badge")).ToHaveCountAsync(0);
            await o.ScreenshotAsync(new() { Path = Path.Combine(evidence, "approved.png"), FullPage = true });
            await owner.Tracing.StopAsync(new() { Path = Path.Combine(evidence, "approval-trace.zip") });
            await using (var check = database.Open())
            {
                Assert.Equal("Bekreftet", (await check.Allocations.SingleAsync()).State);
                Assert.Equal("Utilgjengelig", (await check.Resources.FindAsync(1))!.Status);
            }
            foreach (var page in new[] { p, o })
            {
                await page.Locator("summary.profile-menu-toggle").ClickAsync();
                await page.GetByRole(AriaRole.Button, new() { Name = "Logg ut", Exact = true }).ClickAsync();
                await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Account/Login"));
                await page.GotoAsync(url + "/ResourceProvider");
                await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Account/Login"));
            }
        }
        finally
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            await Task.WhenAll(stdout, stderr);
        }
    }
    private static string BuildConfiguration() => AppContext.BaseDirectory.Contains(Path.DirectorySeparatorChar + "Release" + Path.DirectorySeparatorChar) ? "Release" : "Debug";
    private static async Task Login(IPage page, string url, string email, string password)
    {
        await page.GotoAsync(url + "/Account/Login");
        await page.Locator("#Email").FillAsync(email); await page.Locator("#Password").FillAsync(password);
        await page.Locator("button.auth-submit").ClickAsync();
        await Expect(page.Locator("summary.profile-menu-toggle")).ToBeVisibleAsync();
    }
}
