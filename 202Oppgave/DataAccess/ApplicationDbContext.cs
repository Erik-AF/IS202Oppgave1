using Microsoft.EntityFrameworkCore;
using _202Oppgave.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace _202Oppgave.DataAccess
{
    /// Kobler domenedata og Identity til databasen og definerer relasjoner, slettingsregler og startkategorier.
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

        /// Definerer fremmednøkler, slettingsregler, tidsstempler og faste kategorier; hver allokering peker på én ressurs og ett behov.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Resource>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserId).HasMaxLength(255);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Type).HasMaxLength(100);
                entity.Property(e => e.ImagePath).HasMaxLength(300);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .ValueGeneratedOnAdd();

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Resources)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

                        modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).HasMaxLength(100).IsRequired();

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

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Allocation>(entity =>
            {
                entity.HasKey(a => a.AllocationId);

                entity.Property(a => a.State).HasMaxLength(30).IsRequired();
                entity.Property(a => a.Note).HasMaxLength(500);

                entity.Property(a => a.Date)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .ValueGeneratedOnAdd();

                entity.HasOne(a => a.Resource)
                    .WithMany(r => r.Allocations)
                    .HasForeignKey(a => a.ResourceId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Need)
                    .WithMany(n => n.Allocations)
                    .HasForeignKey(a => a.NeedId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}