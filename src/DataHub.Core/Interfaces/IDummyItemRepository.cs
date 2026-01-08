using DataHub.Core.Models;
using System.Linq;
using System.Threading.Tasks;

namespace DataHub.Core.Interfaces
{
    public interface IDummyItemRepository
    {
        IQueryable<DummyItem> GetDummyItems();
        Task<DummyItem?> GetByIdAsync(int id);
        Task<DummyItem> AddAsync(DummyItem item);
        Task<DummyItem?> UpdateAsync(DummyItem item);
        Task<bool> DeleteAsync(int id);
    }
}