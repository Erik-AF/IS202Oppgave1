using _202Oppgave.Models.Entities;

namespace _202Oppgave.DataAccess.Repositories
{

    public interface IAllocationRepository
    {

        /// Reserverer tilgjengelige ressurser eid av brukeren og oppretter ventende tilbud; returnerer antall tilbud.
        Task<int> OfferAsync(int needId, List<int> resourceIds, string userId, string? note);
        /// Henter allokeringen med behovet som brukes til tilgangskontroll, eller null hvis den mangler.
        Task<Allocation?> GetByIdAsync(int id);
        /// Bekrefter et ventende tilbud og setter ressursen utilgjengelig; returnerer false hvis tilstanden ikke tillater godkjenning.
        Task<bool> ApproveAsync(int id);
        /// Teller ventende tilbud per behov for den oppgitte oppretteren; tom bruker-ID gir ingen treff.
        Task<Dictionary<int, int>> GetPendingCountsByNeedAsync(string userId);

        /// Allokerer valgte ressurser som er tilgjengelige ved uthenting og returnerer antallet som ble lagret.
        Task<int> AllocateAsync(int needId, List<int> resourceIds, string? note);

        /// Henter behovets allokeringer med behov, ressurs og kategori, nyeste først.
        Task<List<Allocation>> GetForNeedAsync(int needId);

        /// Fjerner allokeringen og frigjør ressursen hvis ingen andre allokeringer finnes; returnerer behovets ID eller null.
        Task<int?> RemoveAsync(int allocationId);

        /// Grupperer ressursnavn per behov uten ventende tilbud, med kategori og beskrivelse der dette finnes.
        Task<Dictionary<int, List<string>>> GetResourceNamesByNeedAsync();
    }
}