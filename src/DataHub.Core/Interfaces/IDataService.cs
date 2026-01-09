using DataHub.Core.Models.DataGrid;
using System.Threading.Tasks;

namespace DataHub.Core.Interfaces
{
    public interface IDataService<TItem> where TItem : class
    {
        Task<DataResult<TItem>> GetPagedAsync(DataRequestBase request);
        Task<TItem?> GetByIdAsync(int id);
        Task<TItem> AddAsync(TItem item);
        Task<TItem> UpdateAsync(TItem item);
        Task DeleteAsync(int id);
    }
}
