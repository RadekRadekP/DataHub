using ClosedXML.Excel;
using RPK_BlazorApp.Models.UI;
using RPK_BlazorApp.Models.DataGrid; // Added this line
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataHub.Core.Services
{
    public class ExcelExportService : IExcelExportService
    {
        private readonly ILogger<ExcelExportService> _logger;

        public ExcelExportService(ILogger<ExcelExportService> logger)
        {
            _logger = logger;
        }
        public async Task<Stream> ExportToExcelAsync<TItem>(IEnumerable<TItem> data, List<ColumnDefinition<TItem>> columnDefinitions, string? filterName = null, string? filterString = null) where TItem : class
        {
            var dataList = data.ToList(); // Convert to List to ensure enumeration
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Data");

            // Add headers
            for (int i = 0; i < columnDefinitions.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = columnDefinitions[i].DisplayName;
            }

            // Add data rows
            int row = 2;
            _logger.LogInformation("ExcelExportService: DataList count before foreach loop: {DataListCount}", dataList.Count); // Add this line
            foreach (var item in dataList) // Iterate over the list
            {
                if (item is null) continue;
                for (int i = 0; i < columnDefinitions.Count; i++)
                {
                    var column = columnDefinitions[i];
                    if (column.GetValue != null)
                    {
                        try
                        {
                            var compiledDelegate = column.GetValue.Compile();
                            var value = compiledDelegate.Invoke(item!);
                            _logger.LogInformation("ExcelExportService: Processing Column: {FieldName}, Item Type: {ItemType}, Value: {Value}", column.FieldName, item?.GetType().Name, value);
                            worksheet.Cell(row, i + 1).Value = value?.ToString();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "ExcelExportService: Error processing column {FieldName} for item type {ItemType}", column.FieldName, item?.GetType().Name);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("ExcelExportService: Column {FieldName} GetValue is null.", column.FieldName);
                    }
                }
                row++;
            }

            // Add filter information to a separate sheet if provided
            if (!string.IsNullOrWhiteSpace(filterName) || !string.IsNullOrWhiteSpace(filterString))
            {
                var filterWorksheet = workbook.Worksheets.Add("Filter Info");
                filterWorksheet.Cell(1, 1).Value = "Filter Name:";
                filterWorksheet.Cell(1, 2).Value = filterName;
                filterWorksheet.Cell(2, 1).Value = "Filter String:";
                filterWorksheet.Cell(2, 2).Value = filterString;
            }

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0; // Reset stream position to the beginning
            return await Task.FromResult(stream);
        }
    }
}
