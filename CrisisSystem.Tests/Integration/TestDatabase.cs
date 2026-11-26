using _202Oppgave.DataAccess;
using _202Oppgave.Models.Entities;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Xunit;

namespace CrisisSystem.Tests.Integration;

public sealed class MariaDbFactAttribute : FactAttribute
{
    public MariaDbFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEST_MARIADB_CONNECTION")))
            Skip = "Sett TEST_MARIADB_CONNECTION til en isolert MariaDB-testserver. Se docs/Testing.md.";
    }
}

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly string adminConnection;
    private readonly string name = "crisis_test_" + Guid.NewGuid().ToString("N");
    public string ConnectionString { get; private set; } = "";
    private TestDatabase(string connection) => adminConnection = new MySqlConnectionStringBuilder(connection) { Database = "" }.ConnectionString;

    /// Hver test får en ny database. Ingen eksisterende database tømmes eller gjenbrukes.
    public static async Task<TestDatabase> Create()
    {
        var database = new TestDatabase(Environment.GetEnvironmentVariable("TEST_MARIADB_CONNECTION") ?? throw new InvalidOperationException("Test connection missing"));
        await using var connection = new MySqlConnection(database.adminConnection);
        await connection.OpenAsync();
        await new MySqlCommand($"CREATE DATABASE `{database.name}`", connection).ExecuteNonQueryAsync();
        database.ConnectionString = new MySqlConnectionStringBuilder(database.adminConnection) { Database = database.name }.ConnectionString;
        try
        {
            await using var db = database.Open();
            await db.Database.MigrateAsync();
            db.Users.AddRange(new ApplicationUser { Id = "provider", UserName = "provider", FullName = "Provider" },
                new ApplicationUser { Id = "owner", UserName = "owner", FullName = "Owner" },
                new ApplicationUser { Id = "other", UserName = "other", FullName = "Other" });
            db.Needs.AddRange(new Need { Id = 1, UserId = "owner", Type = "Strøm", Location = "Oslo", Deadline = DateTime.UtcNow.AddDays(1) },
                new Need { Id = 2, UserId = "other", Type = "Vann", Location = "Oslo", Deadline = DateTime.UtcNow.AddDays(1) });
            db.Resources.AddRange(Enumerable.Range(1, 3).Select(id => new Resource { Id = id, UserId = "provider", Type = "Aggregat " + id, Name = "Oslo", Status = "Tilgjengelig" }));
            await db.SaveChangesAsync();
            return database;
        }
        catch { await database.DisposeAsync(); throw; }
    }

    public ApplicationDbContext Open() => new(new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseMySql(ConnectionString, new MariaDbServerVersion(new Version(11, 0, 0))).Options);

    public async ValueTask DisposeAsync()
    {
        await using var connection = new MySqlConnection(adminConnection);
        await connection.OpenAsync();
        await new MySqlCommand($"DROP DATABASE `{name}`", connection).ExecuteNonQueryAsync();
    }
}
