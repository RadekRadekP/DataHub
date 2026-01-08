using DataHub.Core.Models;
using DataHub.Core.Interfaces;
using DataHub.Platform.Data;

namespace DataHub.Core.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog, ApplicationDbContext>
    {
        Task<System.IO.Stream> ExportToExcelAsync(DataHub.Core.Models.UI.DataFilterModel filter);
    }
}
