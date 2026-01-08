using System.Collections.Generic;

namespace DataHub.Core.Services
{
    public interface ICsvExportService
    {
        byte[] ExportToCsv<T>(IEnumerable<T> data);
    }
}