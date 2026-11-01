using _202Oppgave.ViewModels;

namespace _202Oppgave.Models;

// Need representerer et registrert behov, mens NeedViewModel tar imot skjemadata.
public class Need
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public NeedPriority Priority { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
