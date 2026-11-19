using Microsoft.EntityFrameworkCore;
using _202Oppgave.Models.Entities;

namespace _202Oppgave.DataAccess.Repositories
{
    public class AllocationRepository : IAllocationRepository
    {
        private readonly ApplicationDbContext _context;

        public AllocationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tilbud reserverer ressursen, men blir ikke bekreftet før behovets eier godkjenner.
        public async Task<int> OfferAsync(int needId, List<int> resourceIds, string userId, string? note)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            var count = 0;
            foreach (var id in resourceIds.Distinct())
            {
                // Betinget oppdatering hindrer at to innsendinger reserverer samme ressurs.
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

        public async Task<bool> ApproveAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
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
            // Henter bare ressursene som er valgt OG fortsatt tilgjengelige.
            // Da kan ikke en ressurs allokeres to ganger, selv om noen sender
            // skjemaet med en ressurs som allerede er tatt.
            var resources = await _context.Resources
                .Where(r => resourceIds.Contains(r.Id) && r.Status == "Tilgjengelig")
                .ToListAsync();

            foreach (var resource in resources)
            {
                // Ny rad i Allocations for hver ressurs
                _context.Allocations.Add(new Allocation
                {
                    ResourceId = resource.Id,
                    NeedId = needId,
                    State = "Bekreftet",
                    Note = note
                });

                // Ressursen er nå i bruk, og vises som utilgjengelig
                resource.Status = "Utilgjengelig";
            }

            // Én SaveChanges for alt: enten lagres alle endringene, eller ingen
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

            // Ressursen blir tilgjengelig igjen, men bare hvis den ikke
            // er allokert til andre behov også.
            var hasOtherAllocations = await _context.Allocations
                .AnyAsync(a => a.ResourceId == resource.Id && a.AllocationId != allocationId);

            if (!hasOtherAllocations)
            {
                resource.Status = "Tilgjengelig";
            }

            // Én SaveChanges: slettingen og statusendringen lagres sammen
            await _context.SaveChangesAsync();

            return needId;
        }
        // Teller bare ubehandlede tilbud på innlogget brukers egne behov.
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
            // Én spørring: henter bare feltene vi trenger fra Allocations,
            // Resources og Categories (to JOIN-er i SQL)
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

            // Grupperer per behov, og lager f.eks. "Traktor – John Deere 6R"
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