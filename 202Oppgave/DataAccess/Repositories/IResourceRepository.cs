using _202Oppgave.Models.Entities;

namespace _202Oppgave.DataAccess.Repositories;

public interface IResourceRepository
{

    Task AddAsync(Resource resource);

    Task UpdateAsync(Resource resource);

    /// Henter alle ressurser med kategori og eierdata.
    Task<List<Resource>> GetAllAsync();

    Task<Resource?> GetByIdAsync(int id);

    /// Henter brukerens ressurser med kategori, sortert etter synkende ID.
    Task<List<Resource>> GetByUserIdAsync(string userId);

    /// Sletter ressursen hvis den finnes; tilhørende allokeringer slettes av databasens slettingsregel.
    Task DeleteAsync(int id);

    Task<List<Category>> GetCategoriesAsync();
}