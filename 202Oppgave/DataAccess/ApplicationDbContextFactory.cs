using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace _202Oppgave.DataAccess
{
    // Design-time factory for EF Core tools (migrations, scaffolding)
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables();

            var configuration = configBuilder.Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            // Use the project's configured MySQL provider (Pomelo)
            optionsBuilder.UseMySql(
                         connectionString,
                         new MariaDbServerVersion(new Version(11, 0, 0)));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
