using Microsoft.EntityFrameworkCore;
using _202Oppgave.Models.Entities;
namespace _202Oppgave.DataAccess.Repositories
{
    /// Lagrer behov gjennom EF Core og frigjør tilknyttede ressurser ved sletting.
    public class NeedRepository : INeedRepository
    {
        private readonly ApplicationDbContext _context;

        public NeedRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        /// async lar metoden avvente databasearbeid uten å blokkere forespørselstråden.
        /// await sikrer at lagringen er ferdig før metoden fullføres. Task representerer arbeidet.
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
