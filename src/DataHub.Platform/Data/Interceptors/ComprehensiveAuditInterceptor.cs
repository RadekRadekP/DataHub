using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using DataHub.Core.Models;
using DataHub.Core.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DataHub.Platform.Data.Interceptors
{
    public class ComprehensiveAuditInterceptor : SaveChangesInterceptor
    {
        // Prozatím budeme používat pevně dané UserId. V reálné aplikaci byste zde
        // injektovali službu pro získání aktuálního uživatele, např. IHttpContextAccessor.
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
                // 1. Zpracování IAuditableEntity
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
                        // Pokusíme se získat původní hodnotu ChangeCounter tak, jak byla načtena z databáze.
                        // To je klíčové, aby se zabránilo tomu, že DTO-mapovaná hodnota ChangeCounter bude základem pro inkrementaci.
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
                            // Fallback: Pokud původní hodnota nemohla být určena (např. entita byla připojena jako Modified
                            // bez původních hodnot, nebo ChangeCounter je nová vlastnost, která není v DB originálech).
                            // V tomto případě je inkrementace aktuální hodnoty rozumným nouzovým řešením.
                            auditableEntity.ChangeCounter++;
                        }
                    }
                }

                // 2. Příprava AuditLog záznamu (pokud entita není AuditLog samotná)
                if (entry.Entity is not AuditLog &&
                    (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted))
                {
                    var auditEntry = new AuditLog
                    {
                        UserId = DefaultUserId, // Zde byste získali aktuálního uživatele
                        EntityName = entry.Metadata.ClrType.Name,
                        Timestamp = utcNow,
                        PrimaryKey = GetPrimaryKeyValues(entry)
                    };

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.ActionType = "Created";
                            auditEntry.NewValues = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
                            break;

                        case EntityState.Modified:
                            auditEntry.ActionType = "Updated";
                            var oldValuesDict = new Dictionary<string, object?>();
                            var newValuesDict = new Dictionary<string, object?>();
                            var changedColumnsList = new List<string>();

                            foreach (var property in entry.Properties.Where(p => p.IsModified))
                            {
                                oldValuesDict[property.Metadata.Name] = property.OriginalValue;
                                newValuesDict[property.Metadata.Name] = property.CurrentValue;
                                changedColumnsList.Add(property.Metadata.Name);
                            }
                            auditEntry.OldValues = JsonSerializer.Serialize(oldValuesDict);
                            auditEntry.NewValues = JsonSerializer.Serialize(newValuesDict);
                            auditEntry.ChangedColumns = JsonSerializer.Serialize(changedColumnsList);
                            break;

                        case EntityState.Deleted:
                            auditEntry.ActionType = "Deleted";
                            auditEntry.OldValues = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                            break;
                    }
                    auditEntries.Add(auditEntry);
                }
            }

            // Přidání všech nashromážděných AuditLog záznamů do kontextu
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

        private List<string> GetChangedColumnNames(EntityEntry entry)
        {
            var changedColumns = new List<string>();
            foreach (var property in entry.Properties)
            {
                // Pro komplexní typy nebo kolekce by bylo potřeba detailnější porovnání.
                // Toto funguje dobře pro skalární vlastnosti.
                if (property.IsModified)
                {
                    changedColumns.Add(property.Metadata.Name);
                }
            }
            return changedColumns;
        }
    }
}