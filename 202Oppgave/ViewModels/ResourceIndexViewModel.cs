using _202Oppgave.Models;

namespace _202Oppgave.ViewModels;

/*
 * ResourceIndexViewModel – alt ressurslisten (Index) trenger:
 * ressursene som skal vises, hva brukeren har filtrert på,
 * og valgene i nedtrekkslistene.
 */
public class ResourceIndexViewModel
{
    // Ressursene etter filtrering og sortering
    public List<Resource> Resources { get; set; } = new();

    // Det brukeren har valgt (kommer fra adressen, f.eks. ?type=Traktor)
    public string? Q { get; set; }        // søketekst
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? Area { get; set; }
    public string? Sort { get; set; }

    // Valgene i nedtrekkslistene, hentet fra alle ressursene
    public List<string> TypeOptions { get; set; } = new();
    public List<string> StatusOptions { get; set; } = new();
    public List<string> AreaOptions { get; set; } = new();
}