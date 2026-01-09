using DataHub.Core.Models.UI;
using DataHub.Core.Models.DataGrid; // Added this line
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DataHub.Core.Services
{
    public interface IExcelExportService
    {
        Task<Stream> ExportToExcelAsync<TItem>(IEnumerable<TItem> data, List<ColumnDefinition<TItem>> columnDefinitions, string? filterName = null, string? filterString = null) where TItem : class;
    }
}
