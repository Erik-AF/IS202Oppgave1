using _202Oppgave.Models.Entities;

namespace _202Oppgave.DataAccess.Repositories
{

    /// <summary>Definerer lagring og behandling av ressurstilbud og allokeringer.</summary>
    public interface IAllocationRepository
    {

        /// <summary>Reserverer tilgjengelige ressurser eid av brukeren og oppretter ventende tilbud; returnerer antall tilbud.</summary>
        Task<int> OfferAsync(int needId, List<int> resourceIds, string userId, string? note);
        /// <summary>Henter allokeringen med behovet som brukes til tilgangskontroll, eller null hvis den mangler.</summary>
        Task<Allocation?> GetByIdAsync(int id);
        /// <summary>Bekrefter et ventende tilbud og setter ressursen utilgjengelig; returnerer false hvis tilstanden ikke tillater godkjenning.</summary>
        Task<bool> ApproveAsync(int id);
        /// <summary>Teller ventende tilbud per behov for den oppgitte oppretteren; tom bruker-ID gir ingen treff.</summary>
        Task<Dictionary<int, int>> GetPendingCountsByNeedAsync(string userId);

        /// <summary>Allokerer valgte ressurser som er tilgjengelige ved uthenting og returnerer antallet som ble lagret.</summary>
        Task<int> AllocateAsync(int needId, List<int> resourceIds, string? note);

        /// <summary>Henter behovets allokeringer med behov, ressurs og kategori, nyeste først.</summary>
        Task<List<Allocation>> GetForNeedAsync(int needId);

        /// <summary>Fjerner allokeringen og frigjør ressursen hvis ingen andre allokeringer finnes; returnerer behovets ID eller null.</summary>
        Task<int?> RemoveAsync(int allocationId);

        /// <summary>Grupperer ressursnavn per behov uten ventende tilbud, med kategori og beskrivelse der dette finnes.</summary>
        Task<Dictionary<int, List<string>>> GetResourceNamesByNeedAsync();
    }
}