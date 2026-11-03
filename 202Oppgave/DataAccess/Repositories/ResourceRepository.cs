using Microsoft.EntityFrameworkCore;
using _202Oppgave.Models.Entities;

namespace _202Oppgave.DataAccess.Repositories;

public class ResourceRepository : IResourceRepository
{
    private readonly ApplicationDbContext _context;

    public ResourceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Legg til ressurs
    public async Task AddAsync(Resource resource)
    {
        await _context.Resources.AddAsync(resource);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Resource resource)
    {
        _context.Resources.Update(resource);
        await _context.SaveChangesAsync();
    }

    // Hent alle ressurser
    public async Task<List<Resource>> GetAllAsync()
    {
        return await _context.Resources.ToListAsync();
    }

    // Hent ressurs med ID
    public async Task<Resource?> GetByIdAsync(int id)
    {
        return await _context.Resources
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    // Slett ressurs
    public async Task DeleteAsync(int id)
    {
        var resource = await GetByIdAsync(id);

        if (resource != null)
        {
            _context.Resources.Remove(resource);
            await _context.SaveChangesAsync();
        }
    }
}