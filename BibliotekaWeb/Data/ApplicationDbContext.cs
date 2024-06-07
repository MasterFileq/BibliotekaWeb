using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BibliotekaWeb.Models;
using Microsoft.AspNetCore.Identity;

namespace BibliotekaWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Czytelnik> Czytelnik { get; set; }
        public DbSet<Ksiazka> Ksiazka { get; set; }
        public DbSet<Wypozyczenie> Wypozyczenie { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Czytelnik>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<Wypozyczenie>()
                .HasOne(w => w.Czytelnik)
                .WithMany()
                .HasForeignKey(w => w.CzytelnikId);

            modelBuilder.Entity<Wypozyczenie>()
                .HasOne(w => w.Ksiazka)
                .WithMany()
                .HasForeignKey(w => w.KsiazkaId);

            // skala wlasciwosci kara
            modelBuilder.Entity<Wypozyczenie>()
                .Property(w => w.Kara)
                .HasColumnType("decimal(18,2)");
        }
    }
}
