using DataHub.Core.Models.Metadata;
using DataHub.Platform.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataHub.Platform.Services
{
    /// <summary>
    /// Service to discover and register database schema into the metadata catalog.
    /// </summary>
    public class SchemaDiscoveryService
    {
        private readonly ApplicationDbContext _catalogContext;
        private readonly ILogger<SchemaDiscoveryService> _logger;

        public SchemaDiscoveryService(
            ApplicationDbContext catalogContext,
            ILogger<SchemaDiscoveryService> logger)
        {
            _catalogContext = catalogContext;
            _logger = logger;
        }

        /// <summary>
        /// Scans a DbContext and imports all entities into the metadata catalog.
        /// </summary>
        public async Task<int> DiscoverAndImportAsync(DbContext sourceContext, string contextName)
        {
            _logger.LogInformation("Starting schema discovery for {ContextName}", contextName);

            var model = sourceContext.Model;
            int entitiesImported = 0;

            foreach (var entityType in model.GetEntityTypes())
            {
                // Skip shadow entities or entities without a CLR type
                if (entityType.ClrType == null)
                {
                    _logger.LogWarning("Skipping entity with null CLR type: {Name}", entityType.Name);
                    continue;
                }

                var tableName = entityType.GetTableName();
                var schemaName = entityType.GetSchema() ?? "dbo";

                _logger.LogInformation("Discovering entity: {EntityName} -> {Schema}.{Table}",
                    entityType.ClrType.Name, schemaName, tableName);

                // Check if this entity already exists in the catalog
                var existingEntity = await _catalogContext.MetaEntities
                    .FirstOrDefaultAsync(e => e.DbContextName == contextName && e.EntityName == entityType.ClrType.Name);

                MetaEntity metaEntity;
                if (existingEntity != null)
                {
                    _logger.LogInformation("Entity {EntityName} already exists, updating...", entityType.ClrType.Name);
                    metaEntity = existingEntity;
                    metaEntity.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    metaEntity = new MetaEntity
                    {
                        EntityName = entityType.ClrType.Name,
                        TableName = tableName ?? entityType.ClrType.Name,
                        SchemaName = schemaName,
                        DbContextName = contextName,
                        ClrTypeName = entityType.ClrType.FullName,
                        DisplayName = FormatDisplayName(entityType.ClrType.Name),
                        IsView = entityType.GetTableName() != null && entityType.FindPrimaryKey() == null,
                        IsDiscovered = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _catalogContext.MetaEntities.Add(metaEntity);
                    await _catalogContext.SaveChangesAsync(); // Save to get the ID
                }

                // Import fields
                await ImportFieldsAsync(metaEntity, entityType);

                // Import relations
                await ImportRelationsAsync(metaEntity, entityType);

                entitiesImported++;
            }

            await _catalogContext.SaveChangesAsync();

            _logger.LogInformation("Schema discovery completed. Imported {Count} entities from {ContextName}",
                entitiesImported, contextName);

            return entitiesImported;
        }

        private async Task ImportFieldsAsync(MetaEntity metaEntity, IEntityType entityType)
        {
            // First, remove existing relations that reference fields of this entity
            // This prevents FK violations when removing fields
            var relatedRelations = await _catalogContext.MetaRelations
                .Where(r => r.FromEntityId == metaEntity.Id || r.ToEntityId == metaEntity.Id)
                .ToListAsync();
            _catalogContext.MetaRelations.RemoveRange(relatedRelations);
            await _catalogContext.SaveChangesAsync();

            // Remove existing SysViewFields and Default SysViews (FK Constraint)
            // Since we are rebuilding MetaFields, any existing View configurations for them are invalid
            var existingViewFields = await _catalogContext.SysViewFields
                .Where(vf => vf.MetaField.MetaEntityId == metaEntity.Id)
                .ToListAsync();
            _catalogContext.SysViewFields.RemoveRange(existingViewFields);
            
            var defaultViews = await _catalogContext.SysViews
                .Where(v => v.MetaEntityId == metaEntity.Id && v.IsDefault)
                .ToListAsync();
             _catalogContext.SysViews.RemoveRange(defaultViews);
            
            await _catalogContext.SaveChangesAsync();

            // Remove existing fields for this entity (to handle updates)
            var existingFields = await _catalogContext.MetaFields
                .Where(f => f.MetaEntityId == metaEntity.Id)
                .ToListAsync();
            _catalogContext.MetaFields.RemoveRange(existingFields);

            int displayOrder = 0;
            var primaryKeyProperties = entityType.FindPrimaryKey()?.Properties.Select(p => p.Name).ToHashSet() ?? new HashSet<string>();

            foreach (var property in entityType.GetProperties())
            {
                var columnName = property.GetColumnName();
                var sqlType = property.GetColumnType();
                var clrType = property.ClrType;

                var metaField = new MetaField
                {
                    MetaEntityId = metaEntity.Id,
                    FieldName = property.Name,
                    ColumnName = columnName ?? property.Name,
                    DisplayName = FormatDisplayName(property.Name),
                    SqlType = sqlType ?? "unknown",
                    ClrType = clrType.FullName ?? clrType.Name,
                    IsPrimaryKey = primaryKeyProperties.Contains(property.Name),
                    IsForeignKey = property.IsForeignKey(),
                    IsNullable = property.IsNullable,
                    IsComputed = property.ValueGenerated == ValueGenerated.OnAddOrUpdate || property.ValueGenerated == ValueGenerated.OnUpdate,
                    MaxLength = property.GetMaxLength(),
                    Precision = property.GetPrecision(),
                    Scale = property.GetScale(),
                    DisplayOrder = displayOrder++,
                    IsVisibleInGrid = true,
                    IsEditable = !primaryKeyProperties.Contains(property.Name) && property.ValueGenerated == ValueGenerated.Never,
                    UiHint = InferUiHint(clrType, property.Name),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _catalogContext.MetaFields.Add(metaField);
            }

            await _catalogContext.SaveChangesAsync();
        }

        private async Task ImportRelationsAsync(MetaEntity fromEntity, IEntityType entityType)
        {
            // Remove existing relations for this entity
            var existingRelations = await _catalogContext.MetaRelations
                .Where(r => r.FromEntityId == fromEntity.Id)
                .ToListAsync();
            _catalogContext.MetaRelations.RemoveRange(existingRelations);

            foreach (var navigation in entityType.GetNavigations())
            {
                var foreignKey = navigation.ForeignKey;
                if (foreignKey == null) continue;

                var targetEntityType = navigation.TargetEntityType;
                var targetEntity = await _catalogContext.MetaEntities
                    .FirstOrDefaultAsync(e => e.ClrTypeName == targetEntityType.ClrType.FullName);

                if (targetEntity == null)
                {
                    _logger.LogWarning("Target entity {TargetType} not found in catalog for relation from {FromType}",
                        targetEntityType.ClrType.Name, fromEntity.EntityName);
                    continue;
                }

                var fromField = await _catalogContext.MetaFields
                    .FirstOrDefaultAsync(f => f.MetaEntityId == fromEntity.Id && 
                                             f.FieldName == foreignKey.Properties.First().Name);

                var toField = await _catalogContext.MetaFields
                    .FirstOrDefaultAsync(f => f.MetaEntityId == targetEntity.Id && f.IsPrimaryKey);

                if (fromField == null || toField == null)
                {
                    _logger.LogWarning("Could not find fields for relation from {From} to {To}",
                        fromEntity.EntityName, targetEntity.EntityName);
                    continue;
                }

                // Try to find a good display field (Name, Title, etc.)
                var displayField = await FindDisplayFieldAsync(targetEntity);

                var metaRelation = new MetaRelation
                {
                    FromEntityId = fromEntity.Id,
                    FromFieldId = fromField.Id,
                    ToEntityId = targetEntity.Id,
                    ToFieldId = toField.Id,
                    DisplayFieldId = displayField?.Id,
                    RelationType = navigation.IsCollection ? "OneToMany" : "ManyToOne",
                    DisplayName = navigation.Name,
                    DeleteBehavior = foreignKey.DeleteBehavior.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _catalogContext.MetaRelations.Add(metaRelation);
            }

            await _catalogContext.SaveChangesAsync();
            await EnsureDefaultViewAsync(fromEntity);
        }

        private async Task<MetaField?> FindDisplayFieldAsync(MetaEntity entity)
        {
            var candidateNames = new[] { "Name", "Title", "DisplayName", "Nazev", "Description" };

            foreach (var name in candidateNames)
            {
                var field = await _catalogContext.MetaFields
                    .FirstOrDefaultAsync(f => f.MetaEntityId == entity.Id && 
                                             f.FieldName == name);
                if (field != null)
                    return field;
            }

            // Fallback to first non-key string field
            return await _catalogContext.MetaFields
                .Where(f => f.MetaEntityId == entity.Id && 
                           !f.IsPrimaryKey && 
                           f.ClrType == "System.String")
                .OrderBy(f => f.DisplayOrder)
                .FirstOrDefaultAsync();
        }

        private string FormatDisplayName(string name)
        {
            // Simple camel-case to spaced words conversion
            if (string.IsNullOrEmpty(name)) return name;

            var result = new System.Text.StringBuilder();
            result.Append(char.ToUpper(name[0]));

            for (int i = 1; i < name.Length; i++)
            {
                if (char.IsUpper(name[i]) && i > 0 && !char.IsUpper(name[i - 1]))
                    result.Append(' ');
                result.Append(name[i]);
            }

            return result.ToString();
        }

        private string? InferUiHint(Type clrType, string propertyName)
        {
            var typeName = Nullable.GetUnderlyingType(clrType)?.Name ?? clrType.Name;

            if (typeName == "DateTime" || typeName == "DateTimeOffset")
                return "DatePicker";
            if (typeName == "Boolean")
                return "Checkbox";
            if (typeName == "Int32" || typeName == "Int64" || typeName == "Decimal" || typeName == "Double")
                return "NumberField";
            if (propertyName.Contains("Email", StringComparison.OrdinalIgnoreCase))
                return "Email";
            if (propertyName.Contains("Phone", StringComparison.OrdinalIgnoreCase))
                return "Phone";
            if (propertyName.Contains("Url", StringComparison.OrdinalIgnoreCase))
                return "Url";
            if (propertyName.Contains("Color", StringComparison.OrdinalIgnoreCase))
                return "ColorPicker";

            return "TextBox";
        }

        /// <summary>
        /// Registers in-memory entities manually (entities not backed by EF Core DbContext)
        /// </summary>
        public async Task<int> RegisterInMemoryEntitiesAsync()
        {
            _logger.LogInformation("Registering in-memory entities");

            int entitiesRegistered = 0;

            // Register DummyStatus first (referenced by Item)
            entitiesRegistered += await RegisterEntityTypeAsync<DataHub.Core.Models.DummyStatus>("InMemory");

            // Register DummyCategory (referenced by Item)
            entitiesRegistered += await RegisterEntityTypeAsync<DataHub.Core.Models.DummyCategory>("InMemory");
            
            // Register DummyItem (references Status and Category)
            entitiesRegistered += await RegisterEntityTypeAsync<DataHub.Core.Models.DummyItem>("InMemory");

            // Register DummyMeta (clean metadata-driven implementation)
            entitiesRegistered += await RegisterEntityTypeAsync<DataHub.Core.Models.DummyMeta>("InMemory");

            _logger.LogInformation("In-memory registration complete. Registered {Count} entities", entitiesRegistered);
            return entitiesRegistered;
        }

        private async Task<int> RegisterEntityTypeAsync<T>(string contextName) where T : class
        {
            var entityType = typeof(T);
            var entityName = entityType.Name;

            _logger.LogInformation("Registering in-memory entity: {EntityName}", entityName);

            // Check if entity already exists
            var existingEntity = await _catalogContext.MetaEntities
                .FirstOrDefaultAsync(e => e.DbContextName == contextName && e.EntityName == entityName);

            MetaEntity metaEntity;
            if (existingEntity != null)
            {
                _logger.LogInformation("Entity {EntityName} already exists, updating...", entityName);
                metaEntity = existingEntity;
                metaEntity.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                metaEntity = new MetaEntity
                {
                    EntityName = entityName,
                    TableName = entityName,
                    SchemaName = "memory",
                    DbContextName = contextName,
                    ClrTypeName = entityType.FullName,
                    DisplayName = FormatDisplayName(entityName),
                    IsView = false,
                    IsDiscovered = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _catalogContext.MetaEntities.Add(metaEntity);
                await _catalogContext.SaveChangesAsync();
            }

            // First, remove existing relations that reference fields of this entity
            // This prevents FK violations when removing fields
            var relatedRelations = await _catalogContext.MetaRelations
                .Where(r => r.FromEntityId == metaEntity.Id || r.ToEntityId == metaEntity.Id)
                .ToListAsync();
            _catalogContext.MetaRelations.RemoveRange(relatedRelations);
            await _catalogContext.SaveChangesAsync();

            // Remove existing SysViewFields and Default SysViews (FK Constraint)
            var existingViewFields = await _catalogContext.SysViewFields
                .Where(vf => vf.MetaField.MetaEntityId == metaEntity.Id)
                .ToListAsync();
            _catalogContext.SysViewFields.RemoveRange(existingViewFields);
            
            var defaultViews = await _catalogContext.SysViews
                .Where(v => v.MetaEntityId == metaEntity.Id && v.IsDefault)
                .ToListAsync();
            _catalogContext.SysViews.RemoveRange(defaultViews);
            
            await _catalogContext.SaveChangesAsync();

            // Remove existing fields
            var existingFields = await _catalogContext.MetaFields
                .Where(f => f.MetaEntityId == metaEntity.Id)
                .ToListAsync();
            _catalogContext.MetaFields.RemoveRange(existingFields);

            // Reflect on properties
            var properties = entityType.GetProperties();
            int displayOrder = 0;

            foreach (var prop in properties)
            {
                // Filter out non-scalar properties (Navigations, Collections)
                if (prop.PropertyType != typeof(string) && !prop.PropertyType.IsValueType)
                {
                    // Allow byte arrays if needed, but skip complex classes
                    if (prop.PropertyType != typeof(byte[])) continue;
                }

                // Skip [NotMapped] if using annotations (good practice even for in-memory)
                if (prop.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute), true).Any())
                    continue;

                var isPrimaryKey = prop.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), false).Any();
                var isForeignKey = prop.Name.EndsWith("Id") && (prop.PropertyType == typeof(int?) || prop.PropertyType == typeof(int));
                var maxLengthAttr = prop.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.MaxLengthAttribute), false)
                    .FirstOrDefault() as System.ComponentModel.DataAnnotations.MaxLengthAttribute;

                var metaField = new MetaField
                {
                    MetaEntityId = metaEntity.Id,
                    FieldName = prop.Name,
                    ColumnName = prop.Name,
                    DisplayName = FormatDisplayName(prop.Name),
                    SqlType = prop.PropertyType.Name,
                    ClrType = prop.PropertyType.FullName ?? prop.PropertyType.Name,
                    IsPrimaryKey = isPrimaryKey,
                    IsForeignKey = isForeignKey,
                    IsNullable = Nullable.GetUnderlyingType(prop.PropertyType) != null,
                    IsComputed = false,
                    MaxLength = maxLengthAttr?.Length,
                    DisplayOrder = displayOrder++,
                    IsVisibleInGrid = !prop.Name.Contains("Timestamp") && !prop.Name.Contains("ChangeCounter"),
                    IsEditable = !isPrimaryKey,
                    UiHint = InferUiHint(prop.PropertyType, prop.Name),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _catalogContext.MetaFields.Add(metaField);
            }

            await _catalogContext.SaveChangesAsync();

            // Register relations for in-memory entities
            await RegisterInMemoryRelationsAsync(metaEntity, entityType);

            await RegisterInMemoryRelationsAsync(metaEntity, entityType);
            await EnsureDefaultViewAsync(metaEntity);
            return 1;
        }

        private async Task RegisterInMemoryRelationsAsync(MetaEntity fromEntity, Type entityType)
        {
            // Remove existing relations
            var existingRelations = await _catalogContext.MetaRelations
                .Where(r => r.FromEntityId == fromEntity.Id)
                .ToListAsync();
            _catalogContext.MetaRelations.RemoveRange(existingRelations);

            // Look for navigation properties with ForeignKey attribute
            var properties = entityType.GetProperties();
            foreach (var prop in properties)
            {
                var foreignKeyAttr = prop.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute), false)
                    .FirstOrDefault() as System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute;

                if (foreignKeyAttr != null && !prop.PropertyType.IsValueType)
                {
                    // This is a navigation property
                    var targetType = prop.PropertyType;
                    var foreignKeyFieldName = foreignKeyAttr.Name;

                    var targetEntity = await _catalogContext.MetaEntities
                        .FirstOrDefaultAsync(e => e.ClrTypeName == targetType.FullName);

                    if (targetEntity == null)
                    {
                        _logger.LogWarning("Target entity {TargetType} not found for relation from {FromType}",
                            targetType.Name, fromEntity.EntityName);
                        continue;
                    }

                    var fromField = await _catalogContext.MetaFields
                        .FirstOrDefaultAsync(f => f.MetaEntityId == fromEntity.Id && f.FieldName == foreignKeyFieldName);

                    var toField = await _catalogContext.MetaFields
                        .FirstOrDefaultAsync(f => f.MetaEntityId == targetEntity.Id && f.IsPrimaryKey);

                    if (fromField == null || toField == null)
                    {
                        _logger.LogWarning("Could not find fields for relation from {From} to {To}",
                            fromEntity.EntityName, targetEntity.EntityName);
                        continue;
                    }

                    var displayField = await FindDisplayFieldAsync(targetEntity);

                    var metaRelation = new MetaRelation
                    {
                        FromEntityId = fromEntity.Id,
                        FromFieldId = fromField.Id,
                        ToEntityId = targetEntity.Id,
                        ToFieldId = toField.Id,
                        DisplayFieldId = displayField?.Id,
                        RelationType = "ManyToOne",
                        DisplayName = prop.Name,
                        DeleteBehavior = "SetNull",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _catalogContext.MetaRelations.Add(metaRelation);
                }
            }

            await _catalogContext.SaveChangesAsync();
        }

        private async Task EnsureDefaultViewAsync(MetaEntity metaEntity)
        {
            // Check if default view exists
            var existingView = await _catalogContext.SysViews
                .FirstOrDefaultAsync(v => v.MetaEntityId == metaEntity.Id && v.Name == "Default" && v.ViewType == "Grid");

            if (existingView == null)
            {
                _logger.LogInformation("Creating default Grid view for {Entity}", metaEntity.EntityName);
                
                var view = new SysView
                {
                    MetaEntityId = metaEntity.Id,
                    Name = "Default",
                    ViewType = "Grid",
                    IsDefault = true,
                    Description = "Automatically generated default grid",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _catalogContext.SysViews.Add(view);
                await _catalogContext.SaveChangesAsync();

                // Populate fields
                var metaFields = await _catalogContext.MetaFields
                    .Where(f => f.MetaEntityId == metaEntity.Id)
                    .OrderBy(f => f.DisplayOrder)
                    .ToListAsync();

                foreach (var field in metaFields)
                {
                    var viewField = new SysViewField
                    {
                        SysViewId = view.Id,
                        MetaFieldId = field.Id,
                        CustomLabel = field.DisplayName,
                        Order = field.DisplayOrder,
                        IsVisible = field.IsVisibleInGrid,
                        IsReadOnly = !field.IsEditable,
                        CustomUiHint = field.UiHint
                    };
                    _catalogContext.SysViewFields.Add(viewField);
                }
                await _catalogContext.SaveChangesAsync();
            }
        }
    }
}
