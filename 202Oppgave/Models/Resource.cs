using System;

namespace _202Oppgave.Models;

/*
 * Resource.cs – domenemodellen for én registrert ressurs.
 
 * Dette er en ren datamodel
 * Det ResourceController lagrer og viser. Lagres foreløpig i en liste i ResourceController, ikke i database.
 
 * Latitude/Longitude er kontraktpunktet mot kartløsningen:
 */
public class Resource
{
    // Settes automatisk av ResourceController når ressursen opprettes.
    public int Id { get; set; }

    // Ressurstype, f.eks. "Traktor", "Drone", "Sand/grus".
    public string Type { get; set; } = string.Empty;

    // Valgfri utdyping av ressursen.
    public string? Description { get; set; }

    // Geografisk område f.eks. kommune eller stedsnavn.
    public string Area { get; set; } = string.Empty;

    // Posisjon satt via kartet. Nullable siden ikke alle ressurser har fått
    // punkt ennå (kartintegrasjonen er ikke ferdig hos Marius).
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Tidsrom ressursen er tilgjengelig i, begge valgfrie.
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }

    // Kontaktinfo til den som tilbyr ressursen.
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }

    // Tilgjengelig / Reservert / Utilgjengelig. Fritekst foreløpig, samme
    // verdier brukes til fargekoding i kartet (grønn/gul/grå).
    public string Status { get; set; } = "Tilgjengelig";

    // Navn på ressursleverandøren. Erstattes med bruker-id når innlogging
    // er på plass i løsningen.
    public string? OwnerName { get; set; }
}