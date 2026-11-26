
using _202Oppgave.DataAccess;
using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models.Entities;
using _202Oppgave.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<IEmailSender, ConsoleEmailSender>();

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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
