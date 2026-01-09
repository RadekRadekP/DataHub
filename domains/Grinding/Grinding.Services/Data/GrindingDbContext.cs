using Microsoft.EntityFrameworkCore;
using Grinding.Shared.Models;

namespace Grinding.Services.Data
{
    public class GrindingDbContext : DbContext
    {
        public GrindingDbContext(DbContextOptions<GrindingDbContext> options)
            : base(options)
        {
        }

        public DbSet<Operational> OperationalData { get; set; }
        public DbSet<Alarm> Alarms { get; set; }
        public DbSet<Grinding.Shared.Models.Grinding> GrindingData { get; set; } // Explicit specifier to avoid confusion with namespace? No, namespace is Grinding.Services.Data. Class is Grinding.Shared.Models.Grinding.
    }
}
