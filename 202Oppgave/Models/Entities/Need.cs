using _202Oppgave.ViewModels;
namespace _202Oppgave.Models.Entities
{
    public class Need
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public NeedPriority Priority { get; set; }
        public DateTime Deadline { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? GeometryJson { get; set; }
        
        // Tidspunktet behovet ble registrert.
        // Settes automatisk av databasen (DEFAULT CURRENT_TIMESTAMP), ikke av koden.
        public DateTime CreatedAt { get; set; }
        
        // Alle allokeringer til dette behovet (ett behov, mange allokeringer)
        public List<Allocation> Allocations { get; set; } = new();

    }
}
