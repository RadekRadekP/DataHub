using AutoMapper;
using Microsoft.Extensions.Logging;
using RPK_BlazorApp.Models;
using RPK_BlazorApp.Models.DataGrid;
using RPK_BlazorApp.Models.UI;
using RPK_BlazorApp.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RPK_BlazorApp.Services.Generic;
using RPK_BlazorApp.Data;

namespace RPK_BlazorApp.Services
{
    public class AlarmService : DataService<Alarm, AlarmUIModel, DataRequestBase, DataResult<AlarmUIModel>, ApplicationDbContext>, IAlarmService, IAlarmSpecificService
    {
        private readonly IAlarmRepository _alarmRepository;
        private new readonly ILogger<AlarmService> _logger;
        private readonly IExcelExportService _excelExportService;

        public AlarmService(IAlarmRepository alarmRepository, IMapper mapper, ILogger<AlarmService> logger, QueryParserService queryParserService, IExcelExportService excelExportService) : base(alarmRepository, mapper, logger, queryParserService)
        {
            _alarmRepository = alarmRepository;
            _logger = logger;
            _excelExportService = excelExportService;
        }

        async Task<AlarmsPagedResult?> IAlarmSpecificService.GetAlarmsForApiPagedAsync(AlarmFilterModel filter)
        {
            if (filter == null)
            {
                _logger.LogWarning("GetAlarmsForApiPagedAsync called with a null filter.");
                return null;
            }

            // Use the base DataService's GetPagedAsync
            var dataResult = await base.GetPagedAsync(filter.Page, filter.PageSize, filter.RawQuery, false);

            var responseDtos = _mapper.Map<IEnumerable<AlarmRestResponseDTO>>(dataResult.Data);

            return new AlarmsPagedResult
            {
                Alarms = responseDtos,
                TotalCount = dataResult.TotalCount
            };
        }

        async Task<Stream> IAlarmSpecificService.ExportAlarmsToExcelAsync(DataRequestBase request, string? filterName = null, string? filterString = null, List<FilterCriterion>? filters = null, List<SortCriterion>? sorts = null)
        {
            _logger.LogInformation("Service exporting alarms to Excel.");
            try
            {
                // Retrieve all data based on the filter, ignoring pagination
                DataResult<AlarmUIModel> dataResult = await base.GetPagedAsync(1, 1, filterString, true); // Always use rawQuery for consistency

                // Use the ExcelExportService to create the Excel stream
                _logger.LogInformation("AlarmService: ExportAlarmsToExcelAsync - Data count before passing to ExcelExportService: {DataCount}", dataResult.Data.Count());
                return await _excelExportService.ExportToExcelAsync(dataResult.Data, GetColumnDefinitions(), filterName, filterString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service error exporting alarms to Excel.");
                // Return an empty stream in case of an error to avoid crashing the download.
                return new MemoryStream();
            }
        }

        // Helper method to get column definitions for Excel export
        private List<ColumnDefinition<AlarmUIModel>> GetColumnDefinitions()
        {
            return new List<ColumnDefinition<AlarmUIModel>>
            {
                new() { FieldName = nameof(AlarmUIModel.Id), DisplayName = "ID", GetValue = item => item.Id },
                new() { FieldName = nameof(AlarmUIModel.ClientDbId), DisplayName = "Client DB ID", GetValue = item => item.ClientDbId },
                new() { FieldName = nameof(AlarmUIModel.ClientId), DisplayName = "Client ID", GetValue = item => item.ClientId },
                new() { FieldName = nameof(AlarmUIModel.AlarmDate), DisplayName = "Alarm Date", GetValue = item => item.AlarmDate },
                new() { FieldName = nameof(AlarmUIModel.Type), DisplayName = "Type", GetValue = item => item.Type },
                new() { FieldName = nameof(AlarmUIModel.Nr), DisplayName = "Nr", GetValue = item => item.Nr },
                new() { FieldName = nameof(AlarmUIModel.Text), DisplayName = "Text", GetValue = item => item.Text },
                new() { FieldName = nameof(AlarmUIModel.Operator), DisplayName = "Operator", GetValue = item => item.Operator },
                new() { FieldName = nameof(AlarmUIModel.ServerTimestamp), DisplayName = "Server Timestamp", GetValue = item => item.ServerTimestamp },
                new() { FieldName = nameof(AlarmUIModel.ChangeCounter), DisplayName = "Change Counter", GetValue = item => item.ChangeCounter },
            };
        }


    }
}