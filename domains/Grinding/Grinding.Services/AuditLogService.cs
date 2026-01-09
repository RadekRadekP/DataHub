using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DataHub.Core.Data;
using DataHub.Core.Models; // For AuditLog
using DataHub.Core.Models.DataGrid;
using DataHub.Core.Models.UI;
using DataHub.Core.Interfaces; // Assuming repositories are here? No, IAuditLogRepository is in Core
using System;
using System.IO;
using System.Linq;
using AutoMapper;
using System.Text.Json;
using System.Threading.Tasks;
using DataHub.Core.Services;

using DataHub.Platform.Data; // For ApplicationDbContext

namespace Grinding.Services
{
    public class AuditLogService : DataService<AuditLog>, IAuditLogService
    {
        // private new readonly ILogger<AuditLogService> _logger;

        public AuditLogService(IAuditLogRepository repository, IMapper mapper, ILogger<AuditLogService> logger, QueryParserService queryParserService) : base(repository, mapper, logger, queryParserService)
        {
        }

        

        public async Task<Stream> ExportAuditLogsToExcelAsync(DataRequestBase request)
        {
            try
            {
                return await ((IAuditLogRepository)_repository).ExportToExcelAsync((DataFilterModel)request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service error exporting audit logs to Excel.");
                return new MemoryStream();
            }
        }

        

        private string FormatJson(string? jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString)) return string.Empty;
            try
            {
                using var jsonDoc = JsonDocument.Parse(jsonString);
                return JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (JsonException)
            {
                return jsonString; // Return the original string if it's not valid JSON
            }
        }
    }
}