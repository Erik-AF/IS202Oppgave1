using _202Oppgave.DataAccess;
using _202Oppgave.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace CrisisSystem.Tests;

public class NeedUserMappingTests
{
    private static ApplicationDbContext CreateContext() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql("Server=localhost;Database=unused;User=unused;Password=unused",
                new MariaDbServerVersion(new Version(11, 0, 0)))
            .Options);

    [Fact]
    public void NeedUser_ShouldReferenceIdentityAndPreserveLegacyNeeds()
    {
        using var db = CreateContext();
        var need = db.Model.FindEntityType(typeof(Need))!;
        var relationship = Assert.Single(need.GetForeignKeys(), fk => fk.Properties.Any(p => p.Name == "UserId"));
        Assert.Equal(typeof(ApplicationUser), relationship.PrincipalEntityType.ClrType);
        Assert.Equal("Id", Assert.Single(relationship.PrincipalKey.Properties).Name);
        Assert.True(need.FindProperty("UserId")!.IsNullable);
        Assert.Equal(DeleteBehavior.Restrict, relationship.DeleteBehavior);
    }

    [Fact]
    public void MigrationSnapshot_ShouldMatchCurrentModel()
    {
        using var db = CreateContext();
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [Fact]
    public void Migration_ShouldOnlyAddUserColumnIndexAndForeignKey()
    {
        using var db = CreateContext();
        var sql = db.GetService<IMigrator>().GenerateScript(
            "20261113095746_AddUserFullName", "20261114090000_AddNeedUser");
        Assert.Contains("ADD `UserId` varchar(255)", sql);
        Assert.Contains("CREATE INDEX `IX_Needs_UserId`", sql);
        Assert.Contains("REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT", sql);
        Assert.DoesNotContain("DROP TABLE", sql);
        Assert.DoesNotContain("DELETE FROM `Needs`", sql);
    }
}
