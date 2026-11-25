using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using ResourceModel = _202Oppgave.Models.Resource;

namespace _202Oppgave.ViewModels;

/// <summary>Formidler valgte ressurser, behov og merknad; tilbudsmodus bestemmes på serveren.</summary>
public class AllocateViewModel
{
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, ValidateNever]
    public bool IsOffer { get; set; }

    public List<int> ResourceIds { get; set; } = new();

    [Required(ErrorMessage = "Velg et behov")]
    [Display(Name = "Behov")]
    public int? NeedId { get; set; }

    [StringLength(500, ErrorMessage = "Maks 500 tegn")]
    [Display(Name = "Merknad")]
    public string? Note { get; set; }

    [ValidateNever]
    public List<ResourceModel> Resources { get; set; } = new();

    [ValidateNever]
    public List<SelectListItem> NeedOptions { get; set; } = new();
}