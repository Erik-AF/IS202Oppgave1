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
    }
}