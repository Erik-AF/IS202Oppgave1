using _202Oppgave.Models.Entities;
namespace _202Oppgave.DataAccess.Repositories
{
    public interface INeedRepository
    {
        Task AddAsync(Need need);
        Task<List<Need>> GetAllAsync();
        Task<Need?> GetByIdAsync(int id);
        Task UpdateAsync(Need need);
        Task DeleteAsync(int id);
    }
}
