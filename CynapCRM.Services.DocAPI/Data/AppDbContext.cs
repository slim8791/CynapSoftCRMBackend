using CynapCRM.Services.DocAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.DocAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Définition des ensembles d’entités correspondant aux tables principales
        public DbSet<Document> Documents { get; set; }
        public DbSet<Facture> Factures { get; set; }
        public DbSet<BonLivraison> BonsLivraisons { get; set; }
        public DbSet<BonCommande> BonsCommandes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Mise en place du mécanisme TPH (Table Per Hierarchy)
            // Une colonne "TypeDocument" est générée en base afin d’identifier le type de document.
            modelBuilder.Entity<Document>()
                .HasDiscriminator<string>("TypeDocument")
                .HasValue<Document>("Document_Base")
                .HasValue<Facture>("Facture")
                .HasValue<BonLivraison>("BonLivraison")
                .HasValue<BonCommande>("BonCommande");

            // 2. Définition de la clé primaire
            // La propriété Numero_Doc est utilisée comme identifiant unique de chaque document.
            modelBuilder.Entity<Document>()
                .HasKey(d => d.Numero_Doc);

            // 3. Création d’index sur les clés étrangères
            // Ces index améliorent les performances lors des recherches par commande ou par client.
            modelBuilder.Entity<Document>().HasIndex(d => d.Id_Commande);
            modelBuilder.Entity<Document>().HasIndex(d => d.Id_Client);

            // 4. Personnalisation du nom de la table
            // La table est explicitement nommée "T_Documents_Commerciaux" pour plus de clarté.
            modelBuilder.Entity<Document>().ToTable("T_Documents_Commerciaux");
        }
    }
}
