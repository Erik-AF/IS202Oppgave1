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
        public async Task<Dictionary<int, List<string>>> GetResourceNamesByNeedAsync()
        {
            // Én spørring: henter bare feltene vi trenger fra Allocations,
            // Resources og Categories (to JOIN-er i SQL)
            var rows = await _context.Allocations
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