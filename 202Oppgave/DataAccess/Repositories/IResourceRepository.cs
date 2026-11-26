using _202Oppgave.Models.Entities;

namespace _202Oppgave.DataAccess.Repositories;

public interface IResourceRepository
{
    // Legg til en ressurs
    Task AddAsync(Resource resource);
    
    // Oppdaterer en eksisterende ressurs i MariaDB
    Task UpdateAsync(Resource resource);
    
    // Hent alle ressurser
    Task<List<Resource>> GetAllAsync();

    // Hent én ressurs basert på ID
    Task<Resource?> GetByIdAsync(int id);   
    
    Task<List<Resource>> GetByUserIdAsync(string userId);

    // Hent bildet til en ressurs. Null hvis ressursen ikke har bilde i databasen.
    Task<ResourceImage?> GetImageAsync(int resourceId);

    // Lagre bildet til en ressurs. Har den bilde fra før, byttes det ut.
    Task SaveImageAsync(int resourceId, byte[] data, string contentType);

    // Slett en ressurs
    Task DeleteAsync(int id);
    
    //henter kategorier fra databasen
    Task<List<Category>> GetCategoriesAsync();
}