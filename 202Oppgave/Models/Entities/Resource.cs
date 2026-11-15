namespace _202Oppgave.Models.Entities
{
    public class Resource
    {
        public int Id { get; set; }

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
        
        // Sti til opplastet bilde, f.eks. "/uploads/ressurser/abc.jpg".
        // Null betyr at ressursen ikke har eget bilde.
        public string? ImagePath { get; set; }
        
        //fremmednøkkel til Categories-tabellen.
        // Nullable, så ressurser som allerede finnes ikke fpr feil i migrering
        public int? CategoryId { get; set; }
        
        //Selve kategorien. Fyller når vi henter med Include() i reposetory∑
        public Category? Category { get; set; }

        public string? GeometryJson { get; set; }
        
        // Tidspunktet ressursen ble registrert.
        // Settes automatisk av databasen (DEFAULT CURRENT_TIMESTAMP), ikke av koden.
        public DateTime CreatedAt { get; set; }
        

        // Alle allokeringer ressursen er med i (én ressurs, mange allokeringer)
        public List<Allocation> Allocations { get; set; } = new();
    }
}