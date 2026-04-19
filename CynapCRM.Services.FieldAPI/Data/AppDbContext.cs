using CynapCRM.Services.FieldAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.FieldAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Planning_Visite> Plannings { get; set; }
        public DbSet<Visite> Visites { get; set; }
        public DbSet<Rapport_Visite> Rapports { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<Objectif_Delegue> Objectifs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==================================================
            // ✅ 1–1 : Visite ↔ RapportVisite
            // ==================================================
            modelBuilder.Entity<Visite>()
                .HasOne(v => v.Rapport)
                .WithOne(r => r.Visite)
                .HasForeignKey<Rapport_Visite>(r => r.Id_Visite)
                .OnDelete(DeleteBehavior.Cascade);
            // ✅ Si une visite est supprimée → son rapport aussi

            // ==================================================
            // ✅ 1–N : PlanningVisite → Visites
            // ==================================================
            modelBuilder.Entity<Planning_Visite>()
                .HasMany(p => p.Visites)
                .WithOne(v => v.Planning)
                .HasForeignKey(v => v.Id_Planning)
                .OnDelete(DeleteBehavior.SetNull);
            // ✅ Si un planning est supprimé, les visites restent (historique)


            // 4. Recherches fréquentes par délégué
            modelBuilder.Entity<Region>().HasIndex(r => r.Id_User_Delegue);
            modelBuilder.Entity<Objectif_Delegue>().HasIndex(o => o.Id_User_Delegue);
            modelBuilder.Entity<Planning_Visite>().HasIndex(p => p.Id_User_Delegue);
            modelBuilder.Entity<Visite>().HasIndex(v => v.Id_User_Delegue);
            modelBuilder.Entity<Rapport_Visite>().HasIndex(r => r.Id_User_Delegue);

            // 5. Contraintes supplémentaires
            modelBuilder.Entity<Region>().HasIndex(r => r.CodePostal);
        }
    }
}