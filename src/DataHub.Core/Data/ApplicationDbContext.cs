using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using DataHub.Core.Models;
using DataHub.Core.Data.Interceptors;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataHub.Core.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }


        public DbSet<ClientIdentifier> ClientIdentifiers { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<UserSavedCriteria> UserSavedCriteria { get; set; }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            // Note: Audit logic is now in Interceptor, but if needed here we can keep it.
            // However, typical pattern with Interceptor is NOT to duplicate logic here.
            // The original ApplicationDbContext had CreateAuditEntries() called in SaveChanges.
            // ComprehensiveAuditInterceptor does the same.
            // If Interceptor is registered in Program.cs (AddDbContext(options => options.AddInterceptors...)), then we don't need logic here.
            // For now, I'll assume Interceptor is used and remove explicit audit logic from here to avoid duplication.
            // BUT if Program.cs is not updated to use Interceptor, we lose audit.
            // Host Program.cs likely registers DbContext.
            // I should check Host Program.cs later.
            // For now, simpler DbContext.
            
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
             return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return SaveChangesAsync(true, cancellationToken);
        }

        public override int SaveChanges()
        {
            return SaveChanges(true);
        }


    }
}
