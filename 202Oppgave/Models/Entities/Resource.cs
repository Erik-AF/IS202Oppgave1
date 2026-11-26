namespace _202Oppgave.Models.Entities
{
    /// Databaseentitet for en ressurs med eier, kategori, tilgjengelighet og allokeringer.
    public class Resource
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        public ApplicationUser? User { get; set; }

        public string Type { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Name { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableTo { get; set; }

        public string ContactName { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }

        public string Status { get; set; } = "Tilgjengelig";

        public string? OwnerName { get; set; }

        public string? ImagePath { get; set; }

        public ResourceImage? Image { get; set; }

        public int? CategoryId { get; set; }

        public Category? Category { get; set; }

        public string? GeometryJson { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<Allocation> Allocations { get; set; } = new();
    }
}