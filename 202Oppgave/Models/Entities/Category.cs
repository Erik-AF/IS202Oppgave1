namespace _202Oppgave.Models.Entities
{
    /// Representerer en ressurskategori med tilhørende ressurser.
    public class Category
    {

        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<Resource> Resources { get; set; } = new();
    }
}