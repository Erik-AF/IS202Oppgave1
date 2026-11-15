using _202Oppgave.Models.Entities;

namespace _202Oppgave.DataAccess.Repositories
{
    // Lagrer og henter allokeringer (koblingen mellom ressurser og behov)
    public interface IAllocationRepository
    {
        // Allokerer flere ressurser til ett behov i én operasjon.
        // Returnerer hvor mange ressurser som faktisk ble allokert.
        Task<int> AllocateAsync(int needId, List<int> resourceIds, string? note);

        // Henter alle allokeringer for ett behov, med ressursen og kategorien
        Task<List<Allocation>> GetForNeedAsync(int needId);
        
        // Fjerner en allokering og gjør ressursen tilgjengelig igjen.
        // Returnerer id-en til behovet, eller null hvis allokeringen ikke fantes.
        Task<int?> RemoveAsync(int allocationId);
        
        // Henter navnene på allokerte ressurser for alle behov i én spørring.
        // Nøkkelen er NeedId, verdien er listen med ressursnavn.
        Task<Dictionary<int, List<string>>> GetResourceNamesByNeedAsync();
    }
}