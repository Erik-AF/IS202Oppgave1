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

    // Hent alle ressurser, med kategorien
    public async Task<List<Resource>> GetAllAsync()
    {
        return await _context.Resources
            .Include(r => r.Category)
            .Include(r => r.User)
            .ToListAsync();
    }

    // Hent ressurs med ID, med kategorien
    public async Task<Resource?> GetByIdAsync(int id)
    {
        return await _context.Resources
            .Include(r => r.Category)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
    
    // Hent ressursene til én bruker, med kategorien, nyeste først
    public async Task<List<Resource>> GetByUserIdAsync(string userId)
    {
        return await _context.Resources
            .Include(r => r.Category)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.Id)
            .ToListAsync();
    }
    
    
    // Hent bildet til en ressurs.
    // AsNoTracking: vi skal bare lese bildet, ikke endre det.
    public async Task<ResourceImage?> GetImageAsync(int resourceId)
    {
        return await _context.ResourceImages
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.ResourceId == resourceId);
    }

    // Lagre bildet. Finnes det et bilde fra før, oppdateres det i stedet for å lage en ny rad.
    // Det passer med den unike indeksen: én ressurs, ett bilde.
    public async Task SaveImageAsync(int resourceId, byte[] data, string contentType)
    {
        var image = await _context.ResourceImages
            .FirstOrDefaultAsync(i => i.ResourceId == resourceId);

        if (image == null)
        {
            image = new ResourceImage { ResourceId = resourceId };
            _context.ResourceImages.Add(image);
        }

        image.Data = data;
        image.ContentType = contentType;

        await _context.SaveChangesAsync();
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
    
    // Hent alle kategorier, sortert etter navn. Brukes i nedtrekkslisten i skjemaet.
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}