using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DataHub.Platform.Data;
using DataHub.Core.Models.Metadata;

using DataHub.Core.Services;

namespace DataHub.Platform.Services
{

    public class MetadataService : IMetadataService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public MetadataService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // Entities
        public async Task<MetaEntity?> GetEntityByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.MetaEntities.FindAsync(id);
        }

        public async Task<List<MetaEntity>> GetAllEntitiesAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.MetaEntities.OrderBy(e => e.DbContextName).ThenBy(e => e.EntityName).ToListAsync();
        }

        public async Task<MetaEntity> UpdateEntityAsync(MetaEntity entity)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            entity.UpdatedAt = DateTime.UtcNow;
            context.MetaEntities.Update(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        // Fields
        public async Task<List<MetaField>> GetFieldsByEntityIdAsync(int entityId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.MetaFields
                .Where(f => f.MetaEntityId == entityId)
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();
        }

        public async Task<MetaField?> GetFieldByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.MetaFields.FindAsync(id);
        }

        public async Task<MetaField> UpdateFieldAsync(MetaField field)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            field.UpdatedAt = DateTime.UtcNow;
            context.MetaFields.Update(field);
            await context.SaveChangesAsync();
            return field;
        }

        // Relations
        public async Task<List<MetaRelation>> GetAllRelationsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.MetaRelations
                .Include(r => r.FromEntity)
                .Include(r => r.ToEntity)
                .Include(r => r.FromField)
                .Include(r => r.ToField)
                .Include(r => r.DisplayField)
                .ToListAsync();
        }

        public async Task<MetaRelation?> GetRelationByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.MetaRelations
                .Include(r => r.FromEntity)
                .Include(r => r.ToEntity)
                .Include(r => r.FromField)
                .Include(r => r.ToField)
                .Include(r => r.DisplayField)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<MetaRelation> CreateRelationAsync(MetaRelation relation)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            relation.CreatedAt = DateTime.UtcNow;
            relation.UpdatedAt = DateTime.UtcNow;
            context.MetaRelations.Add(relation);
            await context.SaveChangesAsync();
            return relation;
        }

        public async Task<MetaRelation> UpdateRelationAsync(MetaRelation relation)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            relation.UpdatedAt = DateTime.UtcNow;
            context.MetaRelations.Update(relation);
            await context.SaveChangesAsync();
            return relation;
        }

        public async Task DeleteRelationAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var relation = await context.MetaRelations.FindAsync(id);
            if (relation != null)
            {
                context.MetaRelations.Remove(relation);
                await context.SaveChangesAsync();
            }
        }

        // SysViews
        public async Task<SysView?> GetDefaultViewAsync(int metaEntityId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.SysViews
                .Include(v => v.Fields)
                    .ThenInclude(f => f.MetaField)
                .FirstOrDefaultAsync(v => v.MetaEntityId == metaEntityId && v.IsDefault);
        }

        public async Task<SysView?> GetViewByNameAsync(int metaEntityId, string viewName)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.SysViews
                .Include(v => v.Fields)
                    .ThenInclude(f => f.MetaField)
                .FirstOrDefaultAsync(v => v.MetaEntityId == metaEntityId && v.Name == viewName);
        }
    }
}
