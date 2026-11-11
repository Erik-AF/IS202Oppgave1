using Microsoft.EntityFrameworkCore;
using _202Oppgave.DataAccess;
using _202Oppgave.DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;
using _202Oppgave.Models.Authorization;
using _202Oppgave.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register repository
builder.Services.AddScoped<_202Oppgave.DataAccess.Repositories.IResourceRepository, _202Oppgave.DataAccess.Repositories.ResourceRepository>();

// Gjør NeedRepository tilgjengelig for controlleren gjennom INeedRepository.
builder.Services.AddScoped<INeedRepository, NeedRepository>();

// Gjør AllocationRepository tilgjengelig for controllere og ViewComponents.
builder.Services.AddScoped<IAllocationRepository, AllocationRepository>();

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection mangler.");


builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)));


///Registrere Identity
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
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

// Apply EF Core migrations at startup so the database schema is created/updated.
// This runs once during application startup and will throw if migrations cannot be applied.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;


    // Apply EF Core migrations at startup so the database schema is created/updated.
    // This runs once during application startup and will throw if migrations cannot be applied.
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
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

    ///Oppretter Resource Provider, Public Actor og Admin rollene 
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await IdentitySeeder.SeedRolesAsync(roleManager);


    
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var adminPassword = "!blueSideGromp1";
    var resourcePassword = "!RiverScuttle1";
    var publicPassword = "!RedSideraptors1";

    await IdentitySeeder.SeedDevelopmentAdminAsync(userManager, adminPassword);
    await IdentitySeeder.SeedDevelopmentResourceProviderAsync(userManager, resourcePassword);
    await IdentitySeeder.SeedDevelopmentPublicActorAsync(userManager, publicPassword);
}

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();


app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
