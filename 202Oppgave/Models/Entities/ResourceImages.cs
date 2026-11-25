namespace _202Oppgave.Models.Entities
{
    // Bildet til en ressurs, lagret i databasen.
    // Ligger i egen tabell, så ressurslisten ikke henter bildene hver gang.
    public class ResourceImage
    {
        public int Id { get; set; }

        // Fremmednøkkel til ressursen bildet hører til (én ressurs, ett bilde)
        public int ResourceId { get; set; }
        public Resource Resource { get; set; } = null!;

        // Selve bildet som bytes
        public byte[] Data { get; set; } = Array.Empty<byte>();

        // Bildetypen, f.eks. "image/jpeg". Nettleseren trenger den for å vise bildet.
        public string ContentType { get; set; } = string.Empty;
    }
}