namespace _202Oppgave.Models.Entities
{
    public class Category
    {
        
        // en kategori ressurser kan grupperes i, feks: "Lastebil" eller "Bil".
        // Egen tabell i databasen, så nye kategorier kan legges til uten å endre på koden.
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        
        
        public List<Resource> Resources { get; set; } = new();
    }
}