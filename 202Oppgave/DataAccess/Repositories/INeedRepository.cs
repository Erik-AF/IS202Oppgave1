using _202Oppgave.Models.Entities;
namespace _202Oppgave.DataAccess.Repositories
{
    /// <summary>Definerer uthenting og lagring av behov.</summary>
    public interface INeedRepository
    {
        /// <summary>Lagrer et nytt behov.</summary>
        Task AddAsync(Need need);
        /// <summary>Henter alle registrerte behov.</summary>
        Task<List<Need>> GetAllAsync();
        /// <summary>Henter ett behov etter ID, eller null hvis det ikke finnes.</summary>
        Task<Need?> GetByIdAsync(int id);
        /// <summary>Lagrer endringer i et eksisterende behov.</summary>
        Task UpdateAsync(Need need);
        /// <summary>Sletter behovet og tilhørende allokeringer; frigjør ressurser uten allokeringer til andre behov.</summary>
        Task DeleteAsync(int id);
    }
}
