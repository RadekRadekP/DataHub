using DataHub.Core.Models;
using System.IO;
using System.Threading.Tasks;
using DataHub.Core.Interfaces;
using DataHub.Core.Models.UI;
using DataHub.Platform.Data;

namespace DataHub.Core.Interfaces
{
    public interface IAlarmRepository : IRepository<Alarm, ApplicationDbContext>
    {
        Task<Stream> ExportToExcelAsync(DataFilterModel filter);
        Task<int> GetCountAsync();
    }
}