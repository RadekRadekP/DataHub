using Microsoft.EntityFrameworkCore;
using DataHub.Core.Models; // Assuming ViewEisodSd is in RPK_BlazorApp.Models

namespace DataHub.Platform.Data
{
    public class EisodDbContext : DbContext
    {
        public EisodDbContext(DbContextOptions<EisodDbContext> options)
            : base(options)
        {
        }

        // DbSet for your VIEW_EISOD_SD
        public virtual DbSet<ViewEisodSd> ViewEisodSds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ViewEisodSd>(entity =>
            {
                // Define the composite primary key using the Fluent API.
                // This is the correct way to handle keys for views without modifying the entity class.
                entity.HasNoKey();
                entity.ToView("VIEW_EISOD_SD", "export"); // Specify the schema if it's not the default 'dbo'
            });

            // If you have other entities that belong to this specific database (dbPPRoiDIS),
            // you would configure them here as well.
        }
    }
}