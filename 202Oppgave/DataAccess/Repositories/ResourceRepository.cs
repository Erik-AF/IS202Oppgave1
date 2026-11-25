using Microsoft.EntityFrameworkCore;
using _202Oppgave.Models.Entities;

namespace _202Oppgave.DataAccess.Repositories;

/// Lagrer ressurser gjennom EF Core og laster tilhørende kategori- og brukerdata.
public class ResourceRepository : IResourceRepository
{
    private readonly ApplicationDbContext _context;

    public ResourceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

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

    /// Include laster kategori og eier sammen med ressursene, slik at visningen har relaterte data.
    /// ToListAsync utfører spørringen; Where og Include bygger den opp før dette punktet.
    public async Task<List<Resource>> GetAllAsync()
    {
        return await _context.Resources
            .Include(r => r.Category)
            .Include(r => r.User)
            .ToListAsync();
    }

    public async Task<Resource?> GetByIdAsync(int id)
    {
        return await _context.Resources
            .Include(r => r.Category)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Resource>> GetByUserIdAsync(string userId)
    {
        return await _context.Resources
            .Include(r => r.Category)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.Id)
            .ToListAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var resource = await GetByIdAsync(id);

        if (resource != null)
        {
            _context.Resources.Remove(resource);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}