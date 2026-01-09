using DataHub.Core.Models;
using DataHub.Core.Interfaces;
using DataHub.Core.Data;

namespace DataHub.Core.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<System.IO.Stream> ExportToExcelAsync(DataHub.Core.Models.UI.DataFilterModel filter);
    }
}
