using _202Oppgave.ViewModels;
namespace _202Oppgave.Models.Entities
{
    /// <summary>Databaseentitet for et behov med oppretter, prioritet, kartform og allokeringer.</summary>
    public class Need
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public string Type { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public NeedPriority Priority { get; set; }
        public DateTime Deadline { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? GeometryJson { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<Allocation> Allocations { get; set; } = new();

    }
}
