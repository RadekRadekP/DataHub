using DataHub.Core.Models;
using DataHub.Core.Models.DataGrid;
using System.Threading.Tasks;

namespace Grinding.Services
{
    public interface IDummyMetaService
    {
        Task<ServerDataResult<DummyMeta>> GetPagedAsync(ServerDataRequest request);
        Task<DummyMeta?> GetByIdAsync(int id);
        Task<DummyMeta> AddAsync(DummyMeta item);
        Task<DummyMeta> UpdateAsync(DummyMeta item);
        Task DeleteAsync(int id);
    }
}
