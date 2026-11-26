namespace _202Oppgave.Models.Entities
{

    /// Bildedata ligger separat slik at vanlige ressursoppslag ikke laster hele bildet.
    public class ResourceImage
    {
        public int Id { get; set; }

        public int ResourceId { get; set; }
        public Resource Resource { get; set; } = null!;

        public byte[] Data { get; set; } = Array.Empty<byte>();

        public string ContentType { get; set; } = string.Empty;
    }
}