using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CrisisSystem.Tests.Integration;

[Trait("Category", "MariaDB")]
public class AllocationDatabaseTests
{
    [MariaDbFact]
    public async Task Offer_PersistsPendingAllocationAndReservesOnlyOwnedResources()
    {
        await using var database = await TestDatabase.Create(); await using var db = database.Open();
        var repo = new AllocationRepository(db);
        Assert.Equal(0, await repo.OfferAsync(1, new() { 1 }, "other", null));
        Assert.Equal(1, await repo.OfferAsync(1, new() { 1, 1 }, "provider", "Avtalt"));
        await using var check = database.Open();
        var saved = Assert.Single(await check.Allocations.ToListAsync());
        Assert.Equal("Forespurt", saved.State); Assert.Equal("Avtalt", saved.Note);
        Assert.Equal("Reservert", (await check.Resources.FindAsync(1))!.Status);
    }

    [MariaDbFact]
    public async Task Approve_ConfirmsOfferAndMakesResourceUnavailable()
    {
        await using var database = await TestDatabase.Create(); await using var db = database.Open(); var repo = new AllocationRepository(db);
        await repo.OfferAsync(1, new() { 1 }, "provider", null); var id = (await db.Allocations.SingleAsync()).AllocationId;
        Assert.True(await repo.ApproveAsync(id)); Assert.False(await repo.ApproveAsync(id));
        await using var check = database.Open();
        Assert.Equal("Bekreftet", (await check.Allocations.SingleAsync()).State);
        Assert.Equal("Utilgjengelig", (await check.Resources.FindAsync(1))!.Status);
    }

    [MariaDbFact]
    public async Task Approve_WhenResourceStateChanged_RollsBackAllocationUpdate()
    {
        await using var database = await TestDatabase.Create(); await using var db = database.Open(); var repo = new AllocationRepository(db);
        await repo.OfferAsync(1, new() { 1 }, "provider", null); var id = (await db.Allocations.SingleAsync()).AllocationId;
        await db.Resources.Where(r => r.Id == 1).ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, "Utilgjengelig"));
        Assert.False(await repo.ApproveAsync(id));
        await using var check = database.Open(); Assert.Equal("Forespurt", (await check.Allocations.SingleAsync()).State);
    }

    [MariaDbFact]
    public async Task Remove_LastAllocation_ReleasesResource()
    {
        await using var database = await TestDatabase.Create(); await using var db = database.Open(); var repo = new AllocationRepository(db);
        await repo.OfferAsync(1, new() { 1 }, "provider", null); var id = (await db.Allocations.SingleAsync()).AllocationId;
        Assert.Equal(1, await repo.RemoveAsync(id));
        await using var check = database.Open(); Assert.Empty(await check.Allocations.ToListAsync());
        Assert.Equal("Tilgjengelig", (await check.Resources.FindAsync(1))!.Status);
    }

    [MariaDbFact]
    public async Task Remove_WithAnotherAllocation_DoesNotReleaseResource()
    {
        await using var database = await TestDatabase.Create(); await using var db = database.Open();
        db.Allocations.AddRange(new Allocation { NeedId = 1, ResourceId = 1, State = "Bekreftet" }, new Allocation { NeedId = 2, ResourceId = 1, State = "Bekreftet" });
        (await db.Resources.FindAsync(1))!.Status = "Utilgjengelig"; await db.SaveChangesAsync();
        var id = (await db.Allocations.FirstAsync(a => a.NeedId == 1)).AllocationId;
        await new AllocationRepository(db).RemoveAsync(id);
        await using var check = database.Open(); Assert.Single(await check.Allocations.ToListAsync());
        Assert.Equal("Utilgjengelig", (await check.Resources.FindAsync(1))!.Status);
    }

    [MariaDbFact]
    public async Task DeleteNeed_CascadesAllocationsAndOnlyReleasesUnsharedResources()
    {
        await using var database = await TestDatabase.Create(); await using var db = database.Open();
        db.Allocations.AddRange(new Allocation { NeedId = 1, ResourceId = 1 }, new Allocation { NeedId = 1, ResourceId = 2 }, new Allocation { NeedId = 2, ResourceId = 2 });
        foreach (var resource in await db.Resources.Where(r => r.Id <= 2).ToListAsync()) resource.Status = "Utilgjengelig";
        await db.SaveChangesAsync(); await new NeedRepository(db).DeleteAsync(1);
        await using var check = database.Open(); Assert.Null(await check.Needs.FindAsync(1));
        Assert.Equal(2, Assert.Single(await check.Allocations.ToListAsync()).NeedId);
        Assert.Equal("Tilgjengelig", (await check.Resources.FindAsync(1))!.Status);
        Assert.Equal("Utilgjengelig", (await check.Resources.FindAsync(2))!.Status);
    }

    [MariaDbFact]
    public async Task ConcurrentOffers_OnlyOneCanReserveSameResource()
    {
        await using var database = await TestDatabase.Create();
        await using var first = database.Open(); await using var second = database.Open();
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<int> Offer(AllocationRepository repo) { await gate.Task; return await repo.OfferAsync(1, new() { 1 }, "provider", null); }
        var a = Offer(new(first)); var b = Offer(new(second)); gate.SetResult();
        var results = await Task.WhenAll(a, b); Assert.Equal(new[] { 0, 1 }, results.OrderBy(x => x));
        await using var check = database.Open(); Assert.Single(await check.Allocations.ToListAsync());
    }
}

[Trait("Category", "MariaDB")]
public class NotificationDatabaseTests
{
    [MariaDbFact]
    public async Task PendingCounts_ExcludeOtherOwnersAndConfirmedAllocations()
    {
        await using var database = await TestDatabase.Create(); await using var db = database.Open();
        db.Allocations.AddRange(new Allocation { NeedId = 1, ResourceId = 1, State = "Forespurt" }, new Allocation { NeedId = 1, ResourceId = 2, State = "Bekreftet" }, new Allocation { NeedId = 2, ResourceId = 3, State = "Forespurt" });
        await db.SaveChangesAsync(); var counts = await new AllocationRepository(db).GetPendingCountsByNeedAsync("owner");
        Assert.Equal(new KeyValuePair<int, int>(1, 1), Assert.Single(counts));
    }

    [MariaDbFact]
    public async Task PendingCounts_AdminOwnerReceivesOwnNotifications()
    {
        await using var database = await TestDatabase.Create(); await using var db = database.Open();
        db.Roles.Add(new Microsoft.AspNetCore.Identity.IdentityRole { Id = "admin-role", Name = "Admin", NormalizedName = "ADMIN" });
        db.UserRoles.Add(new() { UserId = "owner", RoleId = "admin-role" });
        await db.SaveChangesAsync(); var repo = new AllocationRepository(db);
        await repo.OfferAsync(1, new() { 1, 2 }, "provider", null);
        Assert.Equal(2, (await repo.GetPendingCountsByNeedAsync("owner"))[1]);
        Assert.Empty(await repo.GetPendingCountsByNeedAsync("other"));
    }

    [MariaDbFact]
    public async Task PendingCounts_DisappearAfterApprovalAndRemoval()
    {
        await using var database = await TestDatabase.Create(); await using var db = database.Open(); var repo = new AllocationRepository(db);
        await repo.OfferAsync(1, new() { 1, 2 }, "provider", null);
        var ids = await db.Allocations.OrderBy(a => a.AllocationId).Select(a => a.AllocationId).ToListAsync();
        Assert.True(await repo.ApproveAsync(ids[0])); Assert.Equal(1, (await repo.GetPendingCountsByNeedAsync("owner"))[1]);
        await repo.RemoveAsync(ids[1]); Assert.Empty(await repo.GetPendingCountsByNeedAsync("owner"));
    }

    [MariaDbFact]
    public async Task PendingCounts_EmptyOrUnknownIdentityReceivesNothing()
    {
        await using var database = await TestDatabase.Create(); await using var db = database.Open(); var repo = new AllocationRepository(db);
        await repo.OfferAsync(1, new() { 1 }, "provider", null);
        foreach (var id in new[] { "", " ", "unknown" }) Assert.Empty(await repo.GetPendingCountsByNeedAsync(id));
    }
}

[Trait("Category", "MariaDB")]
public class ResourceImageDatabaseTests
{
    [MariaDbFact]
    public async Task SaveImage_PersistsBytesAndReplacesExistingRow()
    {
        await using var database = await TestDatabase.Create(); await using var db = database.Open(); var repo = new ResourceRepository(db);
        await repo.SaveImageAsync(1, new byte[] { 1, 2 }, "image/png");
        await repo.SaveImageAsync(1, new byte[] { 3, 4, 5 }, "image/jpeg");
        await using var check = database.Open(); var image = Assert.Single(await check.ResourceImages.ToListAsync());
        Assert.Equal(new byte[] { 3, 4, 5 }, image.Data); Assert.Equal("image/jpeg", image.ContentType);
    }

    [MariaDbFact]
    public async Task DeleteResource_CascadesImageAndAllocation()
    {
        await using var database = await TestDatabase.Create(); await using var db = database.Open(); var repo = new ResourceRepository(db);
        await repo.SaveImageAsync(1, new byte[] { 1 }, "image/png");
        await new AllocationRepository(db).OfferAsync(1, new() { 1 }, "provider", null);
        await repo.DeleteAsync(1);
        await using var check = database.Open(); Assert.Null(await check.Resources.FindAsync(1));
        Assert.Empty(await check.ResourceImages.ToListAsync()); Assert.Empty(await check.Allocations.ToListAsync());
    }
}
