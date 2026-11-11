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
    }
}