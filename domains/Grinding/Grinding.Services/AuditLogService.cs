using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RPK_BlazorApp.Data;
using RPK_BlazorApp.Models;
using RPK_BlazorApp.Models.DataGrid;
using RPK_BlazorApp.Models.UI;
using RPK_BlazorApp.Repositories;
using System;
using System.IO;
using System.Linq;
using AutoMapper;
using System.Text.Json;
using System.Threading.Tasks;
using RPK_BlazorApp.Services.Generic;

namespace RPK_BlazorApp.Services
{
    public class AuditLogService : DataService<AuditLog, AuditLog, DataRequestBase, DataResult<AuditLog>, ApplicationDbContext>, IAuditLogService
    {
        private new readonly ILogger<AuditLogService> _logger;

        public AuditLogService(IAuditLogRepository repository, IMapper mapper, ILogger<AuditLogService> logger, QueryParserService queryParserService) : base(repository, mapper, logger, queryParserService)
        {
            _logger = logger;
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