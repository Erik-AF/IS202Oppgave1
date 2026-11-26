using Microsoft.EntityFrameworkCore;
using _202Oppgave.Models.Entities;

namespace _202Oppgave.DataAccess.Repositories
{
    /// Lagrer tilbud og allokeringer med tilhørende tilgjengelighetsstatus gjennom EF Core.
    public class AllocationRepository : IAllocationRepository
    {
        private readonly ApplicationDbContext _context;

        public AllocationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// Reserverer med betinget databaseoppdatering slik at parallelle tilbud ikke tar samme ressurs. Lagrer tilbud og reservasjon i én transaksjon.
        public async Task<int> OfferAsync(int needId, List<int> resourceIds, string userId, string? note)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            var count = 0;
            foreach (var id in resourceIds.Distinct())
            {

                var reserved = await _context.Resources
                    .Where(r => r.Id == id && r.UserId == userId && r.Status == "Tilgjengelig"
                        && !r.Allocations.Any())
                    .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, "Reservert"));
                if (reserved == 0) continue;
                _context.Allocations.Add(new Allocation
                {
                    ResourceId = id, NeedId = needId, State = "Forespurt", Note = note
                });
                count++;
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return count;
        }

        public Task<Allocation?> GetByIdAsync(int id) => _context.Allocations
            .Include(a => a.Need)
            .FirstOrDefaultAsync(a => a.AllocationId == id);

        /// Oppdaterer både tilbud og ressurs betinget i én transaksjon. Manglende tilstandsendring ruller tilbake godkjenningen.
        public async Task<bool> ApproveAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            /// Direkte databaseoppdateringer trenger ikke endringssporing. Uten commit rulles transaksjonen tilbake.
            var allocation = await _context.Allocations.AsNoTracking()
                .FirstOrDefaultAsync(a => a.AllocationId == id);
            if (allocation == null || allocation.State != "Forespurt") return false;

            var approved = await _context.Allocations
                .Where(a => a.AllocationId == id && a.State == "Forespurt")
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.State, "Bekreftet"));
            if (approved == 0) return false;
            var updated = await _context.Resources
                .Where(r => r.Id == allocation.ResourceId && r.Status == "Reservert")
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, "Utilgjengelig"));
            if (updated == 0) return false;
            await transaction.CommitAsync();
            return true;
        }

        public async Task<int> AllocateAsync(int needId, List<int> resourceIds, string? note)
        {

            var resources = await _context.Resources
                .Where(r => resourceIds.Contains(r.Id) && r.Status == "Tilgjengelig")
                .ToListAsync();

            foreach (var resource in resources)
            {

                _context.Allocations.Add(new Allocation
                {
                    ResourceId = resource.Id,
                    NeedId = needId,
                    State = "Bekreftet",
                    Note = note
                });

                resource.Status = "Utilgjengelig";
            }

            await _context.SaveChangesAsync();

            return resources.Count;
        }

        public async Task<List<Allocation>> GetForNeedAsync(int needId)
        {
            return await _context.Allocations
                .Where(a => a.NeedId == needId)
                .Include(a => a.Need)
                .Include(a => a.Resource)
                .ThenInclude(r => r.Category)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }
        public async Task<int?> RemoveAsync(int allocationId)
        {
            var allocation = await _context.Allocations
                .Include(a => a.Resource)
                .FirstOrDefaultAsync(a => a.AllocationId == allocationId);

            if (allocation == null)
            {
                return null;
            }

            var needId = allocation.NeedId;
            var resource = allocation.Resource;

            _context.Allocations.Remove(allocation);

            /// En ressurs kan først frigjøres når ingen andre allokeringer bruker den.
            var hasOtherAllocations = await _context.Allocations
                .AnyAsync(a => a.ResourceId == resource.Id && a.AllocationId != allocationId);

            if (!hasOtherAllocations)
            {
                resource.Status = "Tilgjengelig";
            }

            await _context.SaveChangesAsync();

            return needId;
        }

        public async Task<Dictionary<int, int>> GetPendingCountsByNeedAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return new();
            return await _context.Allocations
                .Where(a => a.State == "Forespurt" && a.Need.UserId == userId)
                .GroupBy(a => a.NeedId)
                .Select(g => new { NeedId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.NeedId, g => g.Count);
        }

        public async Task<Dictionary<int, List<string>>> GetResourceNamesByNeedAsync()
        {

            var rows = await _context.Allocations
                .Where(a => a.State != "Forespurt")
                .Select(a => new
                {
                    a.NeedId,
                    Category = a.Resource.Category != null ? a.Resource.Category.Name : null,
                    a.Resource.Description,
                    a.Resource.Type
                })
                .ToListAsync();

            return rows
                .GroupBy(r => r.NeedId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r =>
                    {
                        var text = string.IsNullOrWhiteSpace(r.Description) ? r.Type : r.Description;
                        return r.Category != null ? $"{r.Category} – {text}" : text;
                    }).ToList());
        }

    }
}