using _202Oppgave.Models.Entities;

namespace _202Oppgave.DataAccess.Repositories;

/// <summary>Definerer uthenting og lagring av ressurser og kategorier.</summary>
public interface IResourceRepository
{

    /// <summary>Lagrer et nytt ressurs.</summary>
    Task AddAsync(Resource resource);

    /// <summary>Lagrer endringer i et eksisterende ressurs.</summary>
    Task UpdateAsync(Resource resource);

    /// <summary>Henter alle ressurser med kategori og eierdata.</summary>
    Task<List<Resource>> GetAllAsync();

    /// <summary>Henter ett ressurs etter ID, eller null hvis det ikke finnes.</summary>
    Task<Resource?> GetByIdAsync(int id);

    /// <summary>Henter brukerens ressurser med kategori, sortert etter synkende ID.</summary>
    Task<List<Resource>> GetByUserIdAsync(string userId);

    /// <summary>Sletter ressursen hvis den finnes; tilhørende allokeringer slettes av databasens slettingsregel.</summary>
    Task DeleteAsync(int id);

    /// <summary>Henter kategorier sortert etter navn til skjemavalg.</summary>
    Task<List<Category>> GetCategoriesAsync();
}