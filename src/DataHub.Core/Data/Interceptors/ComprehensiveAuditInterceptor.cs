using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using DataHub.Core.Models;
using DataHub.Core.Models.Interfaces; // Use correct namespace for IAuditableEntity
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

// Namespace adapted to Core
namespace DataHub.Core.Data.Interceptors 
{
    public class ComprehensiveAuditInterceptor : SaveChangesInterceptor
    {
        // For now using fixed UserId. In real app, inject IHttpContextAccessor.
        private const string DefaultUserId = "SYSTEM";

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            ProcessTrackedEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ProcessTrackedEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ProcessTrackedEntities(DbContext? context)
        {
            if (context == null) return;

            var utcNow = DateTime.UtcNow;
            var auditEntries = new List<AuditLog>();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                // 1. Processing IAuditableEntity
                if (entry.Entity is IAuditableEntity auditableEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        auditableEntity.ServerTimestamp = utcNow;
                        auditableEntity.ChangeCounter = 1;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        auditableEntity.ServerTimestamp = utcNow;

                        object? originalValueObj = null;
                        if (entry.OriginalValues.Properties.Any(p => p.Name == nameof(IAuditableEntity.ChangeCounter)))
                        {
                            originalValueObj = entry.OriginalValues[nameof(IAuditableEntity.ChangeCounter)];
                        }

                        if (originalValueObj is int originalChangeCounter)
                        {
                            auditableEntity.ChangeCounter = originalChangeCounter + 1;
                        }
                        else
                        {
                            auditableEntity.ChangeCounter++;
                        }
                    }
                }

                // 2. Prepare AuditLog entry (if entity is not AuditLog itself)
                if (entry.Entity is not AuditLog &&
                    (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted))
                {
                    var auditEntry = new AuditLog
                    {
                        UserId = DefaultUserId,
                        EntityName = entry.Metadata.ClrType.Name,
                        Timestamp = utcNow,
                        PrimaryKey = GetPrimaryKeyValues(entry)
                    };

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.ActionType = "Created";
                            // Note: ToObject() requires explicit package or helper if strictly Core, but usually EF Core has it.
                            // If missing, we can iterate properties. For now assume it works or replace if error.
                            // Actually ToObject() is not standard EF Core? It might be extension.
                            // Let's use simpler serialization of CurrentValues.Properties
                            auditEntry.NewValues = GetValuesJson(entry, false);
                            break;

                        case EntityState.Modified:
                            auditEntry.ActionType = "Updated";
                            auditEntry.OldValues = GetValuesJson(entry, true);
                            auditEntry.NewValues = GetValuesJson(entry, false);
                            auditEntry.ChangedColumns = GetChangedColumnsJson(entry);
                            break;

                        case EntityState.Deleted:
                            auditEntry.ActionType = "Deleted";
                            auditEntry.OldValues = GetValuesJson(entry, true);
                            break;
                    }
                    auditEntries.Add(auditEntry);
                }
            }

            if (auditEntries.Any())
            {
                context.AddRange(auditEntries);
            }
        }

        private string GetPrimaryKeyValues(EntityEntry entry)
        {
            var primaryKey = entry.Metadata.FindPrimaryKey();
            if (primaryKey == null) return "N/A";

            var pkValues = new Dictionary<string, object?>();
            foreach (var property in primaryKey.Properties)
            {
                pkValues[property.Name] = entry.Property(property.Name).CurrentValue;
            }
            return JsonSerializer.Serialize(pkValues);
        }

        private string? GetValuesJson(EntityEntry entry, bool oldValues)
        {
             var values = new Dictionary<string, object?>();
             foreach (var property in entry.Properties)
             {
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
    }
}
