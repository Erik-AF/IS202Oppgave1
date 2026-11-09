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
            // Tar med ressursene som er allokert til hvert behov
            return await _context.Needs
                .Include(n => n.Resources)
                    .ThenInclude(r => r.Category)
                .ToListAsync();
        }
        public async Task<Need?> GetByIdAsync(int id)
        {
            return await _context.Needs
                .Include(n => n.Resources)
                    .ThenInclude(r => r.Category)
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
                // Frigjør ressursene som var allokert, så de kan brukes igjen.
                // NeedId settes til null automatisk (DeleteBehavior.SetNull).
                foreach (var resource in need.Resources)
                {
                    resource.Status = "Tilgjengelig";
                }

                _context.Needs.Remove(need);
                await _context.SaveChangesAsync();
            }
        }
    }
}
