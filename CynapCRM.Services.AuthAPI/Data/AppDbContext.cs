using CynapCRM.Services.AuthAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.AuthAPI.Data
{
    public class AppDbContext : IdentityDbContext<Utilisateur, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Utilisateur> Utilisateurs { get; set; }

        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //Héritage avec discriminant (TPH)
            modelBuilder.Entity<Utilisateur>()
                .HasDiscriminator<string>("UserType")
                .HasValue<Administrateur>("Admin")
                .HasValue<Superviseur>("Superviseur")
                .HasValue<Delegue>("Delegue")
                .HasValue<Medecin>("Medecin")
                .HasValue<Pharmacien>("Pharmacien")
                .HasValue<Grossiste>("Grossiste");

            //Données initiales (seed data)
            modelBuilder.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int> { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole<int> { Id = 2, Name = "Superviseur", NormalizedName = "SUPERVISEUR" },
                new IdentityRole<int> { Id = 3, Name = "Delegue", NormalizedName = "DELEGUE" },
                new IdentityRole<int> { Id = 4, Name = "Medecin", NormalizedName = "MEDECIN" },
                new IdentityRole<int> { Id = 5, Name = "Client", NormalizedName = "CLIENT" }
            );
            

        }
    }
}
