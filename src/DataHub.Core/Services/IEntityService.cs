
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataHub.Core.Services.Generic
{
    public interface IEntityService<TItem> where TItem : class
    {
        Task<TItem?> GetByIdAsync(object id);
        Task<List<TItem>> GetAllAsync();
        Task AddAsync(TItem item);
        Task UpdateAsync(TItem item);
        Task DeleteAsync(object id);
        Task<int> GetCountAsync();
    }
}
