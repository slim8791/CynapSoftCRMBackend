using CynapCRM.Services.SupportAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.SupportAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Reclamation> Reclamations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Conversion pour une lecture claire dans SQL Server
            modelBuilder.Entity<Reclamation>()
                .Property(r => r.Type)
                .HasConversion<string>();

            modelBuilder.Entity<Reclamation>()
                .Property(r => r.NiveauUrgence)
                .HasConversion<string>();
        }
    }
}
        