using System.Linq.Expressions;
using DataHub.Core.Models.DataGrid;

namespace DataHub.Core.Models.Interfaces
{
    public interface IDataService<TItem> where TItem : class
    {
        Task<TItem?> GetByIdAsync(int id);
        Task<TItem> AddAsync(TItem item);
        Task<TItem> UpdateAsync(TItem item);
        Task DeleteAsync(int id);
        Task<ServerDataResult<TItem>> GetPagedAsync(ServerDataRequest request, Expression<Func<TItem, bool>>? filter = null);
    }
}
