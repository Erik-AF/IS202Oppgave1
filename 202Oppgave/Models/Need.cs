using _202Oppgave.ViewModels;

namespace _202Oppgave.Models;

/// Visningsmodell for behovsdata. Lagring håndteres av Models.Entities.Need.
public class Need
{
    public int Id { get; set; }
    public bool CanManage { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public NeedPriority Priority { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string? GeometryJson { get; set; }
}
