using DataHub.Core.Interfaces;
using DataHub.Core.Models;
using DataHub.Core.Models.DataGrid;
using System.IO;
using System.Threading.Tasks;

namespace Grinding.Services
{
    public interface IAuditLogService : IDataService<AuditLog>
    {
        Task<Stream> ExportAuditLogsToExcelAsync(DataRequestBase request);
    }
}
