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

            //  1–1 : Visite ↔ RapportVisite
            modelBuilder.Entity<Visite>()
                .HasOne(v => v.Rapport)
                .WithOne(r => r.Visite)
                .HasForeignKey<Rapport_Visite>(r => r.Id_Visite)
                .OnDelete(DeleteBehavior.Cascade); // If a visit is deleted → its report is also deleted

            //  1–N : PlanningVisite → Visites
            modelBuilder.Entity<Planning_Visite>()
                .HasMany(p => p.Visites)
                .WithOne(v => v.Planning)
                .HasForeignKey(v => v.Id_Planning)
                .OnDelete(DeleteBehavior.SetNull); // If a planning is deleted, the visits remain (historical)


            // 4. Frequent searches by delegate/superviseur
            modelBuilder.Entity<Region>().HasIndex(r => r.Id_Superviseur);
            modelBuilder.Entity<Objectif_Delegue>().HasIndex(o => o.Id_User_Delegue);
            modelBuilder.Entity<Planning_Visite>().HasIndex(p => p.Id_User_Delegue);
            modelBuilder.Entity<Visite>().HasIndex(v => v.Id_User_Delegue);
            modelBuilder.Entity<Rapport_Visite>().HasIndex(r => r.Id_User_Delegue);

            // 5. Additional constraints
            modelBuilder.Entity<Region>().HasIndex(r => r.CodePostal);
        }
    }
}