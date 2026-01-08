using RPK_BlazorApp.Models.UI;
using RPK_BlazorApp.Models.DataGrid; // Added this line
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
