using System.ComponentModel.DataAnnotations;

namespace _202Oppgave.Models;

// Dataene brukeren fyller inn i kontaktskjemaet

public class KontaktFormModel
{
    public string Navn { get; set; }
    public string Epost { get; set; }
    public string Emne { get; set; }
    public string Melding { get; set; }

    // konstruktør som blir fyllt inn av brukeren
    public KontaktFormModel()
    {
        Navn = "";
        Epost = "";
        Emne = "";
        Melding = "";
    }

    // Konstruktør for når vi lager objektet selv
    public KontaktFormModel(string navn, string epost, string emne, string melding)
    {
        Navn = navn;
        Epost = epost;
        Emne = emne;
        Melding = melding;
    }
}          

