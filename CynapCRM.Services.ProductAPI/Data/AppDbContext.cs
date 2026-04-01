using CynapCRM.Services.ProductAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

namespace CynapCRM.Services.ProductAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        // table de catalogue 
        public DbSet<Produit> Produits { get; set; }
        public DbSet<Lot> Lots { get; set; }
        public DbSet<Promotion> Promotions { get; set; }

        //table supportMarketing
        public DbSet<Support_Marketting> Support_Markettings { get; set; }
        public DbSet<Fichier> Fichiers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configuration de la relation Produit -> Lots (1-N)
            modelBuilder.Entity<Lot>()
                .HasOne(l => l.Produit)
                .WithMany(p => p.Lots)
                .HasForeignKey(l => l.Id_Produit)
                .OnDelete(DeleteBehavior.Cascade); // Si on supprime un produit, on supprime ses lots

            // 2. Configuration de la relation Lot -> Promotions (1-N)

            modelBuilder.Entity<Promotion>()
                .HasOne(promo => promo.Lot)
                .WithMany(l => l.Promotions)
                .HasForeignKey(promo => promo.NumeroLot)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Configuration Produit -> Supports (1-N)
            modelBuilder.Entity<Support_Marketting>()
                .HasOne(s => s.Produit)
                .WithMany(p => p.Supports)
                .HasForeignKey(s => s.Id_Produit)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. Configuration Support -> Fichiers (1-N)
            modelBuilder.Entity<Fichier>()
                .HasOne(f => f.Support)
                .WithMany(s => s.Fichiers)
                .HasForeignKey(f => f.Id_Support)
                .OnDelete(DeleteBehavior.Cascade);
            
        }
    }
}