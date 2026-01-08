using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataHub.Core.Services
{
    public class CsvExportService : ICsvExportService
    {
        public byte[] ExportToCsv<T>(IEnumerable<T> data)
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead)
                .ToList();

            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
            {
                // Write header
                var header = string.Join(";", properties.Select(p => p.GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name));
                writer.WriteLine(header);

                // Write data rows
                foreach (var item in data)
                {
                    var values = properties.Select(p =>
                    {
                        var value = p.GetValue(item, null)?.ToString() ?? "";
                        // Escape quotes and handle values containing the separator
                        if (value.Contains(';') || value.Contains('"'))
                        {
                            return $"\"{value.Replace("\"", "\"\"")}\"";
                        }
                        return value;
                    });
                    writer.WriteLine(string.Join(";", values));
                }
            }
            return memoryStream.ToArray();
        }
    }
}