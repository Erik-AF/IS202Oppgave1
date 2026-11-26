using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CrisisSystem.Tests.Integration;

[Trait("Category", "MariaDB")]
public class RepositoryDatabaseTests
{
    [MariaDbFact]
    public async Task Need_CreateUpdateReadDelete_PersistsAcrossContexts()
    {
        await using var database = await TestDatabase.Create(); int id;
        await using (var db = database.Open())
        {
            var need = new Need { UserId = "owner", Type = "Mat", Location = "Oslo", Description = "Test", Deadline = DateTime.UtcNow.AddDays(1) };
            await new NeedRepository(db).AddAsync(need); id = need.Id;
        }
        await using (var db = database.Open())
        {
            var repo = new NeedRepository(db); var need = await repo.GetByIdAsync(id); Assert.NotNull(need);
            Assert.Equal("Mat", need.Type); need.Type = "Vann"; await repo.UpdateAsync(need);
        }
        await using (var db = database.Open())
        {
            var repo = new NeedRepository(db); Assert.Equal("Vann", (await repo.GetByIdAsync(id))!.Type);
            await repo.DeleteAsync(id);
        }
        await using var check = database.Open(); Assert.Null(await new NeedRepository(check).GetByIdAsync(id));
    }

    [MariaDbFact]
    public async Task Resource_CreateUpdateReadDelete_PersistsRelationsAndDoesNotReuseExistingId()
    {
        await using var database = await TestDatabase.Create(); int id;
        await using (var db = database.Open())
        {
            var resource = new Resource { UserId = "provider", Type = "Bil", Name = "Oslo", CategoryId = 1 };
            await new ResourceRepository(db).AddAsync(resource); id = resource.Id;
        }
        await using (var db = database.Open())
        {
            var repo = new ResourceRepository(db); var resource = await repo.GetByIdAsync(id); Assert.NotNull(resource);
            Assert.Equal("Bil", resource.Category!.Name); Assert.Equal("provider", resource.User!.Id);
            resource.Type = "Varebil"; await repo.UpdateAsync(resource);
        }
        await using (var db = database.Open())
        {
            var repo = new ResourceRepository(db); Assert.Equal("Varebil", (await repo.GetByIdAsync(id))!.Type);
            await repo.DeleteAsync(id);
            var replacement = new Resource { Type = "Ny bil", Name = "Oslo", UserId = "provider" };
            await repo.AddAsync(replacement); Assert.NotEqual(id, replacement.Id);
        }
        await using var check = database.Open(); Assert.Null(await check.Resources.FindAsync(id));
    }
}
