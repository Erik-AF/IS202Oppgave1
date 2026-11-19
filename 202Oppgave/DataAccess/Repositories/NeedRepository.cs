using Microsoft.EntityFrameworkCore;
using _202Oppgave.Models.Entities;
namespace _202Oppgave.DataAccess.Repositories
{
    public class NeedRepository : INeedRepository
    {
        private readonly ApplicationDbContext _context;

        public NeedRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Need need)
        {
            await _context.Needs.AddAsync(need);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Need>> GetAllAsync()
        {
            return await _context.Needs.ToListAsync();
        }
        public async Task<Need?> GetByIdAsync(int id)
        {
            return await _context.Needs
                .FirstOrDefaultAsync(n => n.Id == id);
        }
        public async Task UpdateAsync(Need need)
        {
            _context.Needs.Update(need);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var need = await GetByIdAsync(id);

            if (need != null)
            {
                // Tilbud og allokeringer forsvinner med behovet; frigjør ressursene samtidig.
                var resources = await _context.Resources
                    .Where(r => r.Allocations.Any(a => a.NeedId == id)
                        && !r.Allocations.Any(a => a.NeedId != id))
                    .ToListAsync();
                foreach (var resource in resources)
                    resource.Status = "Tilgjengelig";
                _context.Needs.Remove(need);
                await _context.SaveChangesAsync();
            }
        }
    }
}
