using CynapCRM.Services.InventoryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.InventoryAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // 🔹 DbSets
        public DbSet<Stock_Delegue> StocksDelegues { get; set; }

        // ❌ SUPPRIMÉ (car héritage TPH → une seule table suffit)
        // public DbSet<Stock_Gratuite> StocksGratuites { get; set; }
        // public DbSet<Stock_Echantillon> StocksEchantillons { get; set; }

        public DbSet<Echantillon> Echantillons { get; set; }

        // ✅ Correction naming (Pas camelCase pour DbSet)
        public DbSet<StockMovement> StockMovements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===========================
            // 🔥 1. Héritage TPH
            // ===========================
            modelBuilder.Entity<Stock_Delegue>()
                .HasDiscriminator<string>("TypeStock")
                .HasValue<Stock_Delegue>("Standard")
                .HasValue<Stock_Echantillon>("Echantillon")
                .HasValue<Stock_Gratuite>("Gratuite");

            // ===========================
            // 🔥 2. Clés primaires explicites (BONNE PRATIQUE)
            // ===========================
            modelBuilder.Entity<Stock_Delegue>()
                .HasKey(s => s.Id_stock);

            modelBuilder.Entity<Echantillon>()
                .HasKey(e => e.Id_Distribution);

            modelBuilder.Entity<StockMovement>()
                .HasKey(m => m.Id);

            // ===========================
            // 🔥 3. Index (Performance 🚀)
            // ===========================
            modelBuilder.Entity<Stock_Delegue>().HasIndex(s => s.NumeroLot);
            modelBuilder.Entity<Echantillon>().HasIndex(e => e.NumeroLot);

            modelBuilder.Entity<Stock_Delegue>().HasIndex(s => s.Id_User_Delegue);
            modelBuilder.Entity<Echantillon>().HasIndex(e => e.Id_Medecin);

            // ✅ AJOUT important
            modelBuilder.Entity<StockMovement>().HasIndex(m => m.IdStock);

            // ===========================
            // 🔥 4. Contraintes (DATA CLEAN 🔐)
            // ===========================
            modelBuilder.Entity<Stock_Delegue>()
                .Property(s => s.NumeroLot)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Echantillon>()
                .Property(e => e.NumeroLot)
                .IsRequired()
                .HasMaxLength(100);

            // ===========================
            // 🔥 5. Relations
            // ===========================
            // ✅ AJOUT : relation StockMovement → Stock
            modelBuilder.Entity<StockMovement>()
                .HasOne<Stock_Delegue>()
                .WithMany()
                .HasForeignKey(m => m.IdStock)
                .OnDelete(DeleteBehavior.Cascade);

            // ===========================
            // 🔥 6. Valeurs par défaut
            // ===========================
            // ✅ AJOUT
            modelBuilder.Entity<Stock_Delegue>()
                .Property(s => s.QteReservee)
                .HasDefaultValue(0);

            // ✅ AJOUT
            modelBuilder.Entity<Echantillon>()
                .Property(e => e.DateDistribution)
                .HasDefaultValueSql("GETUTCDATE()");

            // ✅ AJOUT
            modelBuilder.Entity<StockMovement>()
                .Property(m => m.DateMovement)
                .HasDefaultValueSql("GETUTCDATE()");

            // ===========================
            // 🔥 7. Noms des tables
            // ===========================
            modelBuilder.Entity<Stock_Delegue>().ToTable("Stocks");
            modelBuilder.Entity<Echantillon>().ToTable("Distributions_Echantillons");

            // ✅ AJOUT
            modelBuilder.Entity<StockMovement>().ToTable("Stock_Movements");
        }
    }
}