using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace _202Oppgave.ViewModels;

public class NeedAllocationViewModel
{
    public int NeedId { get; set; }
    public List<int> ResourceIds { get; set; } = new();
    [StringLength(500, ErrorMessage = "Maks 500 tegn")]
    public string? Note { get; set; }
    [BindNever, ValidateNever]
    public string NeedTitle { get; set; } = string.Empty;
    [BindNever, ValidateNever]
    public string Location { get; set; } = string.Empty;
    [BindNever, ValidateNever]
    public List<NearbyResourceViewModel> Suggestions { get; set; } = new();
}

public class NearbyResourceViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Location { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
}
