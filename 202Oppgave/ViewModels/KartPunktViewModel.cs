namespace _202Oppgave.ViewModels;

public class KartPunktViewModel
{
    public int Id { get; set; }
    public string Kategori { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Prioritet  { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Kontaktpunkt { get; set; } = string.Empty;
    public string Beskrivelse  { get; set; } = string.Empty;
}

public class RegistrerPunktViewModel
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Kategori { get; set; } = "behov";
    public string Type { get; set; } = string.Empty;
    public string Beskrivelse { get; set; } = string.Empty;    
}