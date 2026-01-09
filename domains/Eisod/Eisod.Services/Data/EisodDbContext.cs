using Microsoft.EntityFrameworkCore;
using Eisod.Shared.Models;
using System.Reflection;

namespace Eisod.Services.Data
{
    public class EisodDbContext : DbContext
    {
        public EisodDbContext(DbContextOptions<EisodDbContext> options)
            : base(options)
        {
        }

        public DbSet<ViewEisodSd> ViewEisodSds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ViewEisodSd>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("VIEW_EISOD_SD", "export"); 
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
