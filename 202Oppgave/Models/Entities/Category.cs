namespace _202Oppgave.Models.Entities
{
    /// <summary>Representerer en ressurskategori med tilhørende ressurser.</summary>
    public class Category
    {

        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<Resource> Resources { get; set; } = new();
    }
}