using CynapCRM.Services.OrderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.OrderAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Commande> Commandes { get; set; }
        public DbSet<LigneCommande> LignesCommandes { get; set; }
        public DbSet<Reclamation> Reclamations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Relation Commande → Lignes de commande
            // Une commande peut contenir plusieurs lignes. La suppression d’une commande entraîne
            // automatiquement la suppression des lignes associées (DeleteBehavior.Cascade).
            modelBuilder.Entity<LigneCommande>()
                .HasOne(l => l.Commande)
                .WithMany(c => c.Lignes)
                .HasForeignKey(l => l.Id_Commande)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Relation Commande → Réclamations
            // Une commande peut être associée à plusieurs réclamations. La suppression d’une commande
            // entraîne également la suppression des réclamations liées (DeleteBehavior.Cascade).
            modelBuilder.Entity<Reclamation>()
                .HasOne(r => r.Commande)
                .WithMany(c => c.Reclamations)
                .HasForeignKey(r => r.Id_Commande)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Indexation pour l’optimisation des performances
            // Les index créés sur Id_Client, Id_Produit et NumeroLot permettent d’accélérer les recherches
            // et les jointures avec d’autres microservices (par exemple Product ou Client).
            modelBuilder.Entity<Commande>().HasIndex(c => c.Id_Client);
            modelBuilder.Entity<LigneCommande>().HasIndex(l => l.Id_Produit);
            modelBuilder.Entity<LigneCommande>().HasIndex(l => l.NumeroLot);
        }
    }
}
