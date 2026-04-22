using CynapCRM.Services.InventoryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.InventoryAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Stock_Delegue> StocksDelegues { get; set; }


        public DbSet<Echantillon> Echantillons { get; set; }

        public DbSet<StockMovement> StockMovements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Héritage TPH
            modelBuilder.Entity<Stock_Delegue>()
                .HasDiscriminator<string>("TypeStock")
                .HasValue<Stock_Delegue>("Standard")
                .HasValue<Stock_Echantillon>("Echantillon")
                .HasValue<Stock_Gratuite>("Gratuite");

            // Clés primaires explicites
            modelBuilder.Entity<Stock_Delegue>()
                .HasKey(s => s.Id_stock);

            modelBuilder.Entity<Echantillon>()
                .HasKey(e => e.Id_Distribution);

            modelBuilder.Entity<StockMovement>()
                .HasKey(m => m.Id_Movement);

            // Index (Performance )
            modelBuilder.Entity<Stock_Delegue>().HasIndex(s => s.NumeroLot);
            modelBuilder.Entity<Echantillon>().HasIndex(e => e.NumeroLot);

            modelBuilder.Entity<Stock_Delegue>().HasIndex(s => s.Id_User_Delegue);
            modelBuilder.Entity<Echantillon>().HasIndex(e => e.Id_Medecin);
            modelBuilder.Entity<Echantillon>().HasIndex(e => e.Id_Pharmacien);
            modelBuilder.Entity<StockMovement>().HasIndex(m => m.Id_Stock);

            // Contraintes (data clean )
            modelBuilder.Entity<Stock_Delegue>()
                .Property(s => s.NumeroLot)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Echantillon>()
                .Property(e => e.NumeroLot)
                .IsRequired()
                .HasMaxLength(100);

            // Relations


            modelBuilder.Entity<StockMovement>()
                .HasOne<Stock_Delegue>()
                .WithMany()
                .HasForeignKey(m => m.Id_Stock)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Stock_Delegue>()
                .Property(s => s.QteReservee)
                .HasDefaultValue(0);

            modelBuilder.Entity<Echantillon>()
                .Property(e => e.DateDistribution)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<StockMovement>()
                .Property(m => m.DateMovement)
                .HasDefaultValueSql("GETUTCDATE()");

            
            // Noms des tables
            modelBuilder.Entity<Stock_Delegue>().ToTable("Stocks");
            modelBuilder.Entity<Echantillon>().ToTable("Distributions_Echantillons");

            modelBuilder.Entity<StockMovement>().ToTable("Stock_Movements");
        }
    }
}