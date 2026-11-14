using Microsoft.EntityFrameworkCore;
using _202Oppgave.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace _202Oppgave.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {   
        }

        public DbSet<Resource> Resources { get; set; }
        public DbSet<Need> Needs { get; set; }
        public DbSet<Category> Categories { get; set; }
        
        public DbSet<Allocation> Allocations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Resource>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Type).HasMaxLength(100);
                entity.Property(e => e.ImagePath).HasMaxLength(300);
                // Databasen fyller inn tidspunktet når en ny ressurs settes inn
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .ValueGeneratedOnAdd();
                
                // Én kategori har mange ressurser.
                // Slettes en kategori, blir CategoryId satt til null i stedet for at ressursene slettes.
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Resources)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            
                        modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).HasMaxLength(100).IsRequired();

                // Startdata: kategoriene finnes i databasen fra første oppstart.
                // HasData krever at Id settes manuelt.
                entity.HasData(
                    new Category { Id = 1, Name = "Bil" },
                    new Category { Id = 2, Name = "Lastebil" },
                    new Category { Id = 3, Name = "Traktor" },
                    new Category { Id = 4, Name = "Anleggsmaskin" },
                    new Category { Id = 5, Name = "Drone" },
                    new Category { Id = 6, Name = "Helikopter" },
                    new Category { Id = 7, Name = "Båt" },
                    new Category { Id = 8, Name = "Aggregat" },
                    new Category { Id = 9, Name = "Materialer" },
                    new Category { Id = 10, Name = "Personell/ferdighet" },
                    new Category { Id = 11, Name = "Annet" }
                );
            });

            modelBuilder.Entity<Need>(entity =>
            {
                entity.Property(e => e.UserId).HasMaxLength(255);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Databasen fyller inn tidspunktet når et nytt behov settes inn
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .ValueGeneratedOnAdd();
            });

            // Allocation: koblingstabell mellom Resource og Need (mange-til-mange),
            // slik som i ER-diagrammet.
            modelBuilder.Entity<Allocation>(entity =>
            {
                entity.HasKey(a => a.AllocationId);

                entity.Property(a => a.State).HasMaxLength(30).IsRequired();
                entity.Property(a => a.Note).HasMaxLength(500);

                // Databasen setter tidspunktet når allokeringen lages
                entity.Property(a => a.Date)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .ValueGeneratedOnAdd();

                // Én ressurs har mange allokeringer.
                // Slettes ressursen, slettes allokeringene også.
                entity.HasOne(a => a.Resource)
                    .WithMany(r => r.Allocations)
                    .HasForeignKey(a => a.ResourceId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Ett behov har mange allokeringer.
                // Slettes behovet, slettes allokeringene også.
                entity.HasOne(a => a.Need)
                    .WithMany(n => n.Allocations)
                    .HasForeignKey(a => a.NeedId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}