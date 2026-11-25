using System.ComponentModel.DataAnnotations;

namespace _202Oppgave.Models;

/// <summary>Inneholder og validerer opplysningene i kontaktskjemaet.</summary>
public class KontaktFormModel
{
    [Required(ErrorMessage = "Du må fylle inn navnet ditt.")]
    public string Navn { get; set; } = "";

    [Required(ErrorMessage = "Du må fylle inn e-postadressen din.")]
    [EmailAddress(ErrorMessage = "Du må oppgi en gyldig e-postadresse.")]
    public string Epost { get; set; } = "";

    [Required(ErrorMessage = "Du må velge et emne.")]
    public string Emne { get; set; } = "";

    [Required(ErrorMessage = "Du må skrive en melding.")]
    public string Melding { get; set; } = "";

    /// <summary>Oppretter en tom kontaktmodell for skjemabinding.</summary>
    public KontaktFormModel()
    {
    }

    /// <summary>Oppretter skjemamodellen med oppgitte kontaktfelt.</summary>
    public KontaktFormModel(string navn, string epost, string emne, string melding)
    {
        Navn = navn;
        Epost = epost;
        Emne = emne;
        Melding = melding;
    }
}

