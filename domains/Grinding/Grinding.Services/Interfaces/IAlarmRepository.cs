using Grinding.Shared.Models;
using System.IO;
using System.Threading.Tasks;
using DataHub.Core.Interfaces;
using DataHub.Core.Models.UI;
using Grinding.Services.Data;

namespace Grinding.Services.Interfaces
{
    public interface IAlarmRepository : IRepository<Alarm>
    {
        Task<Stream> ExportToExcelAsync(DataFilterModel filter);
        Task<int> GetCountAsync();
    }
}
