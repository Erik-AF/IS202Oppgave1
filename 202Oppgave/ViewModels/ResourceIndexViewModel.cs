using _202Oppgave.Models;

namespace _202Oppgave.ViewModels;

public class ResourceIndexViewModel
{

    public List<Resource> Resources { get; set; } = new();

    public string? Q { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public string? Area { get; set; }
    public string? Sort { get; set; }

    public List<string> CategoryOptions { get; set; } = new();
    public List<string> StatusOptions { get; set; } = new();
    public List<string> AreaOptions { get; set; } = new();
}