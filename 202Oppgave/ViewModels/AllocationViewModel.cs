using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using ResourceModel = _202Oppgave.Models.Resource;

namespace _202Oppgave.ViewModels;

/*
 * AllocateViewModel – data til siden der valgte ressurser allokeres til et behov.
 * Brukes av AllocationController (Create GET og POST).
 */
public class AllocateViewModel
{
    // Id-ene til ressursene som ble krysset av i ressurslisten.
    // Sendes med skjemaet som skjulte felt.
    public List<int> ResourceIds { get; set; } = new();

    // Behovet ressursene skal allokeres til
    [Required(ErrorMessage = "Velg et behov")]
    [Display(Name = "Behov")]
    public int? NeedId { get; set; }

    // Valgfri merknad, f.eks. "Hentes ved låven kl. 08"
    [StringLength(500, ErrorMessage = "Maks 500 tegn")]
    [Display(Name = "Merknad")]
    public string? Note { get; set; }

    // --- Bare til visning. Fylles av controlleren, sendes ikke tilbake fra skjemaet. ---

    // Ressursene som er valgt, så brukeren ser hva som allokeres
    [ValidateNever]
    public List<ResourceModel> Resources { get; set; } = new();

    // Valgene i nedtrekkslisten med behov
    [ValidateNever]
    public List<SelectListItem> NeedOptions { get; set; } = new();
}