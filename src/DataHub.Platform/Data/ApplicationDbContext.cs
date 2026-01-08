using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using DataHub.Core.Models; // Vaše entity modely, včetně AuditLog
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DataHub.Platform.Data.Interceptors; // Moved using statement to the top

namespace DataHub.Platform.Data // <--- Ujistěte se, že tento namespace je správný
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Operational> OperationalData { get; set; }
        public DbSet<Alarm> Alarms { get; set; }
        public DbSet<Grinding> GrindingData { get; set; }
        public DbSet<ClientIdentifier> ClientIdentifiers { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; } // Přidáno pro auditní záznamy
        public DbSet<ViewEisodSd> ViewEisodSds { get; set; }
        public DbSet<UserSavedCriteria> UserSavedCriteria { get; set; }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var auditEntries = CreateAuditEntries();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            
            if (auditEntries.Any())
            {
                // Pokud jsou primární klíče generovány databází, mohou být nyní dostupné
                // a lze je případně aktualizovat v auditEntries před uložením.
                // Pro jednoduchost tento příklad předpokládá, že PK jsou již nastaveny nebo nejsou pro audit kritické před prvním uložením.
                this.AuditLogs.AddRange(auditEntries);
                await base.SaveChangesAsync(cancellationToken); // Uložit auditní záznamy
            }
            
            return result;
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var auditEntries = CreateAuditEntries();
            var result = base.SaveChanges(acceptAllChangesOnSuccess);

            if (auditEntries.Any())
            {
                this.AuditLogs.AddRange(auditEntries);
                base.SaveChanges(); // Uložit auditní záznamy
            }
            return result;
        }

        // Pohodlnější přetížení, pokud ne vždy předáváte acceptAllChangesOnSuccess
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return SaveChangesAsync(true, cancellationToken);
        }

        public override int SaveChanges()
        {
            return SaveChanges(true);
        }

        private List<AuditLog> CreateAuditEntries()
        {
            ChangeTracker.DetectChanges(); // Důležité zavolat před iterací záznamů
            var entries = new List<AuditLog>();
            var utcNow = DateTime.UtcNow;

            // TODO: Implementujte získání skutečného ID uživatele.
            // V ASP.NET Core byste typicky použili _httpContextAccessor.HttpContext.User.Identity.Name
            // nebo jiný mechanismus pro získání ID přihlášeného uživatele.
            var userId = "SYSTEM_USER_PLACEHOLDER";
            if (_httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
                userId = _httpContextAccessor.HttpContext.User.Identity.Name ?? userId;
            }

            foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity is not AuditLog &&
                                                               (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)))
            {
                var auditEntry = new AuditLog
                {
                    UserId = userId,
                    EntityName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                    Timestamp = utcNow,
                    PrimaryKey = GetPrimaryKeyValues(entry),
                };

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.ActionType = "Created";
                        auditEntry.NewValues = GetValuesJson(entry, oldValues: false);
                        break;
                    case EntityState.Modified:
                        auditEntry.ActionType = "Updated";
                        auditEntry.OldValues = GetValuesJson(entry, oldValues: true);
                        auditEntry.NewValues = GetValuesJson(entry, oldValues: false);
                        auditEntry.ChangedColumns = GetChangedColumnsJson(entry);

                        // Přeskočit logování, pokud se žádné hodnoty vlastností nezměnily.
                        // EF může označit entitu jako Modified, i když hodnoty vlastností jsou stejné.
                        if (auditEntry.OldValues == auditEntry.NewValues && string.IsNullOrEmpty(auditEntry.ChangedColumns))
                        {
                            continue; 
                        }
                        break;
                    case EntityState.Deleted:
                        auditEntry.ActionType = "Deleted";
                        auditEntry.OldValues = GetValuesJson(entry, oldValues: true);
                        break;
                }
                entries.Add(auditEntry);
            }
            return entries;
        }

        private string GetPrimaryKeyValues(EntityEntry entry)
        {
            var primaryKey = entry.Metadata.FindPrimaryKey();
            if (primaryKey == null) return "N/A";

            return string.Join(",", primaryKey.Properties
                                       .Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? "null"));
        }

        private string? GetValuesJson(EntityEntry entry, bool oldValues)
        {
            var values = new Dictionary<string, object?>();
            foreach (var property in entry.Properties)
            {
                // Pro stav 'Modified' a 'NewValues' zahrnout pouze vlastnosti, které se skutečně změnily.
                // Pro 'OldValues' nebo stavy 'Added'/'Deleted' zahrnout všechny vlastnosti.
                if (entry.State == EntityState.Modified && !oldValues && !property.IsModified)
                {
                    continue;
                }
                values[property.Metadata.Name] = oldValues ? property.OriginalValue : property.CurrentValue;
            }
            return values.Any() ? JsonSerializer.Serialize(values, new JsonSerializerOptions { WriteIndented = false }) : null;
        }

        private string? GetChangedColumnsJson(EntityEntry entry)
        {
            var changedProperties = entry.Properties.Where(p => p.IsModified).Select(p => p.Metadata.Name).ToList();
            return changedProperties.Any() ? JsonSerializer.Serialize(changedProperties) : null;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Zde můžete konfigurovat vaše entity, pokud je to potřeba
            // Například:
            // modelBuilder.Entity<Grinding>()
            //     .Property(g => g.Value)
            //     .HasColumnType("decimal(18, 4)");

            // Můžete také explicitně nastavit název tabulky pro AuditLog, pokud je to potřeba
            // modelBuilder.Entity<AuditLog>().ToTable("AuditLogs"); // Výchozí název je obvykle v pořádku

            // Configure the ViewEisodSd view
            modelBuilder.Entity<ViewEisodSd>(entity => {
                entity.HasNoKey(); // Configure as keyless entity
                entity.ToView("VIEW_EISOD_SD", "export");
            });
        }
    }
}
