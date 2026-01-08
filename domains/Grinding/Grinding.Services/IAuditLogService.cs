using RPK_BlazorApp.Models;
using RPK_BlazorApp.Models.DataGrid;
using System.IO;
using System.Threading.Tasks;
using RPK_BlazorApp.Services.Generic;
using RPK_BlazorApp.Data;

namespace RPK_BlazorApp.Services
{
    public interface IAuditLogService : IDataService<AuditLog, AuditLog, DataRequestBase, DataResult<AuditLog>, ApplicationDbContext>
    {
        Task<Stream> ExportAuditLogsToExcelAsync(DataRequestBase request);
    }
}
