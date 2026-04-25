using CynapCRM.Services.DocAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.DocAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Definition of the sets of entities corresponding to the main tables
        public DbSet<Document> Documents { get; set; }
        public DbSet<Facture> Factures { get; set; }
        public DbSet<BonLivraison> BonsLivraisons { get; set; }
        public DbSet<BonCommande> BonsCommandes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Setting up the TPH (Table Per Hierarchy) mechanism
            // A "TypeDocument" column is generated in the database to identify the type of document.
            modelBuilder.Entity<Document>()
                .HasDiscriminator<string>("TypeDocument")
                .HasValue<Document>("GENERIC")
                .HasValue<Facture>("FACTURE")
                .HasValue<BonLivraison>("BL")
                .HasValue<BonCommande>("BC");

            // 2. Definition of the primary key
            // The Numero_Doc property is used as the unique identifier for each document.
            modelBuilder.Entity<Document>()
                .HasKey(d => d.Numero_Doc);

            // 3. Creating indexes on foreign keys
            // Improves performance when searching by order or by client.
            modelBuilder.Entity<Document>().HasIndex(d => d.Id_Commande);
            modelBuilder.Entity<Document>().HasIndex(d => d.Id_Client);

            // 4. Customizing the table name
            modelBuilder.Entity<Document>().ToTable("T_Documents_Commerciaux");
        }
    }
}
