
using _202Oppgave.DataAccess;
using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models.Entities;
using _202Oppgave.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using _202Oppgave.Models.Authorization;
using _202Oppgave.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

///Fjernet pga smart kontroller problem
///builder.AddServiceDefaults();

// CSRF-beskyttelse på alle POST-er i hele appen (som i Espens Program.cs).
// Sjekker tokenet på POST, PUT, PATCH og DELETE, men ikke på GET.
builder.Services.AddControllersWithViews(o =>
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

builder.Services.AddScoped<_202Oppgave.DataAccess.Repositories.IResourceRepository, _202Oppgave.DataAccess.Repositories.ResourceRepository>();

builder.Services.AddScoped<INeedRepository, NeedRepository>();

builder.Services.AddScoped<IAllocationRepository, AllocationRepository>();

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection mangler.");

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;

        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(5);

        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<SecurityStampValidatorOptions>(
    options =>
    {
        options.ValidationInterval = TimeSpan.FromMinutes(1);

        options.OnRefreshingPrincipal = context =>
        {
            var amr = context.CurrentPrincipal?
                .FindFirst("amr");

            if (amr is not null &&
                context.NewPrincipal?.Identity
                    is ClaimsIdentity identity &&
                !identity.HasClaim(amr.Type, amr.Value))
            {
                identity.AddClaim(
                    new Claim(amr.Type, amr.Value));
            }

            return Task.CompletedTask;
        };
    });

builder.Services.AddScoped<IEmailSender, ConsoleEmailSender>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";

    options.AccessDeniedPath = "/Account/AccessDenied";

    options.Cookie.HttpOnly = true;

    options.Cookie.SameSite = SameSiteMode.Lax;

    options.Cookie.SecurePolicy =
        builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
});

builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.TwoFactorUserIdScheme,
    options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

        options.Cookie.HttpOnly = true;

        options.Cookie.SecurePolicy =
            builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
    });

var app = builder.Build();


/// Eget scope sørger for at databasekonteksten frigjøres etter migrering og opprettelse av startdata.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        /// Bruker utestående migreringer. Feil kastes videre slik at appen ikke starter med feil skjema.
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetService<ILogger<Program>>();
        if (logger != null)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
        }
        else
        {
            Console.Error.WriteLine($"An error occurred while applying database migrations: {ex}");
        }
        throw;
    }

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await IdentitySeeder.SeedRolesAsync(roleManager);

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    var adminPassword = "!blueSideGromp1";
    var resourcePassword = "!RiverScuttle1";
    var publicPassword = "!RedSideraptors1";

    await IdentitySeeder.SeedDevelopmentAdminAsync(userManager, adminPassword);
    await IdentitySeeder.SeedDevelopmentResourceProviderAsync(userManager, resourcePassword);
    await IdentitySeeder.SeedDevelopmentPublicActorAsync(userManager, publicPassword);
}

///Fjernet pga smart kontroller problem
//app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var headers = context.Response.Headers;
        headers["X-Frame-Options"] = "SAMEORIGIN";
        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        return Task.CompletedTask;
    });

    await next(context);
});
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseMiddleware<RequireMfaMiddleware>();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
