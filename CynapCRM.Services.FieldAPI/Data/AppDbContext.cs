using CynapCRM.Services.FieldAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.FieldAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<PlanningVisite> Plannings { get; set; }
        public DbSet<Tournee> Tournees { get; set; }
        public DbSet<Visite> Visites { get; set; }
        public DbSet<Rapport_visite> Rapports { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<Objectif_Delegue> Objectifs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // 1. Relation 1-1 : Visite <-> Rapport_visite
            modelBuilder.Entity<Visite>()
                .HasOne(v => v.Rapport)
                .WithOne(r => r.Visite)
                .HasForeignKey<Rapport_visite>(r => r.Id_Visite)
                .OnDelete(DeleteBehavior.Cascade); // Si on supprime la visite, le rapport part avec.

            // 2. Relation 1-N : Planning -> Tournees
            modelBuilder.Entity<Tournee>()
                .HasOne(t => t.Planning)
                .WithMany(p => p.Tournees)
                .HasForeignKey(t => t.Id_Planning)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Relation 1-N : Tournee -> Visites
            modelBuilder.Entity<Visite>()
                .HasOne(v => v.Tournee)
                .WithMany(t => t.Visites)
                .HasForeignKey(v => v.Id_Tournee)
                .OnDelete(DeleteBehavior.SetNull); // Si on supprime une tournée, on garde les visites (historique) mais sans lien.

            // 4. Recherches fréquentes par délégué
            modelBuilder.Entity<Region>().HasIndex(r => r.Id_User_Delegue);
            modelBuilder.Entity<Objectif_Delegue>().HasIndex(o => o.Id_User_Delegue);
            modelBuilder.Entity<PlanningVisite>().HasIndex(p => p.Id_User_Delegue);
            modelBuilder.Entity<Visite>().HasIndex(v => v.Id_User_Delegue);
            modelBuilder.Entity<Rapport_visite>().HasIndex(r => r.Id_User_Delegue);

            // 5. Contraintes supplémentaires
            modelBuilder.Entity<Region>().HasIndex(r => r.CodePostal);
        }
    }
}