using _202Oppgave.Models.Entities;

namespace _202Oppgave.DataAccess.Repositories;

public interface IResourceRepository
{
    // Legg til en ressurs
    Task AddAsync(Resource resource);

    // Hent alle ressurser
    Task<List<Resource>> GetAllAsync();

    // Hent én ressurs basert på ID
    Task<Resource?> GetByIdAsync(int id);

    // Slett en ressurs
    Task DeleteAsync(int id);
}