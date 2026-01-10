using System.Collections.Generic;
using System.Threading.Tasks;
using DataHub.Core.Models.Metadata;

namespace DataHub.Core.Services
{
    public interface IMetadataService
    {
        // Entities
        Task<MetaEntity?> GetEntityByIdAsync(int id);
        Task<List<MetaEntity>> GetAllEntitiesAsync();
        Task<MetaEntity> UpdateEntityAsync(MetaEntity entity);

        // Fields
        Task<List<MetaField>> GetFieldsByEntityIdAsync(int entityId);
        Task<MetaField?> GetFieldByIdAsync(int id);
        Task<MetaField> UpdateFieldAsync(MetaField field);

        // Relations
        Task<List<MetaRelation>> GetAllRelationsAsync();
        Task<MetaRelation?> GetRelationByIdAsync(int id);
        Task<MetaRelation> CreateRelationAsync(MetaRelation relation);
        Task<MetaRelation> UpdateRelationAsync(MetaRelation relation);
        Task DeleteRelationAsync(int id);

        // SysViews
        Task<SysView?> GetDefaultViewAsync(int metaEntityId);
        Task<SysView?> GetViewByNameAsync(int metaEntityId, string viewName);
    }
}
