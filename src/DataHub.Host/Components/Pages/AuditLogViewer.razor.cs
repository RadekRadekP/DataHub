using Microsoft.AspNetCore.Components;
using DataHub.Core.Models;
using DataHub.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DataHub.Core.Models.UI;
using DataHub.Core.Models.DataGrid;
using System.Linq;
using Microsoft.Extensions.Logging;
using System;
using DataHub.Core.Components.Shared;

namespace DataHub.Host.Components.Pages
{
    public partial class AuditLogViewer
    {
        [Inject] private IAuditLogService _auditLogService { get; set; } = default!;
        [Inject] private ILogger<AuditLogViewer> _logger { get; set; } = default!;
        [Inject] private IExcelExportService _excelExportService { get; set; } = default!;
        [Inject] public UserPreferenceService UserPreferenceService { get; set; } = default!;
        [Inject] public QueryParserService QueryParser { get; set; } = default!;
        [Inject] public NavigationManager NavigationManager { get; set; } = default!;

        private List<AuditLog> auditLogs = new();
        
        private string _advancedQuery = "";
        private string? _activeFilterName;
        private HashSet<AuditLog> _selectedLogs = new();

        private GenericDataView<AuditLog>? _dataView;

        private List<DataHub.Core.Models.DataGrid.ColumnDefinition<AuditLog>> columnDefinitions = new(); // Keep as DataGrid type
        private List<DataHub.Core.Models.UI.ColumnDefinition> _queryBuilderColumns = new(); // New property for UI type

        private bool _isQueryBuilderVisible = false;
        private List<UserSavedCriteria> _savedQueries = new();

        protected override async Task OnInitializedAsync()
        {
            columnDefinitions = new List<DataHub.Core.Models.DataGrid.ColumnDefinition<AuditLog>>
            {
                new() { FieldName = nameof(AuditLog.Id), DisplayName = "Id", GetValue = item => item.Id, DataType = typeof(int), IsSortable = true, IsFilterable = true, IsVisible = true },
                new() { FieldName = nameof(AuditLog.Timestamp), DisplayName = "Timestamp", GetValue = item => item.Timestamp, DataType = typeof(DateTime), IsSortable = true, IsFilterable = true, IsVisible = true },
                new() { FieldName = nameof(AuditLog.ActionType), DisplayName = "Action Type", GetValue = item => item.ActionType ?? string.Empty, DataType = typeof(string), IsSortable = true, IsFilterable = true, IsVisible = true },
                new() { FieldName = nameof(AuditLog.EntityName), DisplayName = "Entity Name", GetValue = item => item.EntityName ?? string.Empty, DataType = typeof(string), IsSortable = true, IsFilterable = true, IsVisible = true },
                new() { FieldName = nameof(AuditLog.PrimaryKey), DisplayName = "Primary Key", GetValue = item => item.PrimaryKey ?? string.Empty, DataType = typeof(string), IsSortable = true, IsFilterable = true, IsVisible = true },
                new() { FieldName = nameof(AuditLog.OldValues), DisplayName = "Old Values", GetValue = item => item.OldValues ?? string.Empty, DataType = typeof(string), IsSortable = false, IsFilterable = false, IsVisible = true },
                new() { FieldName = nameof(AuditLog.NewValues), DisplayName = "New Values", GetValue = item => item.NewValues ?? string.Empty, DataType = typeof(string), IsSortable = false, IsFilterable = false, IsVisible = true },
                new() { FieldName = nameof(AuditLog.ChangedColumns), DisplayName = "Changed Columns", GetValue = item => item.ChangedColumns ?? string.Empty, DataType = typeof(string), IsSortable = false, IsFilterable = false, IsVisible = true },
                new() { FieldName = nameof(AuditLog.UserId), DisplayName = "User ID", GetValue = item => item.UserId ?? string.Empty, DataType = typeof(string), IsSortable = true, IsFilterable = true, IsVisible = true }
            };

            // Populate _queryBuilderColumns by mapping from columnDefinitions
            _queryBuilderColumns = columnDefinitions.Select(c => new DataHub.Core.Models.UI.ColumnDefinition
            {
                FieldName = c.FieldName,
                DisplayName = c.DisplayName,
                DataType = c.DataType,
                IsSortable = c.IsSortable,
                IsFilterable = c.IsFilterable
            }).ToList();

            await LoadSavedQueries();
        }

        private async Task<DataResult<AuditLog>> LoadAuditLogData(DataRequestBase request)
        {
            _logger.LogInformation("AuditLogViewer: LoadAuditLogData called. Page: {Page}, PageSize: {PageSize}, RawQuery: {RawQuery}", request.Page, request.PageSize, _advancedQuery);
            var result = await _auditLogService.GetPagedAsync(request.Page, request.PageSize, rawQuery: _advancedQuery);
            _logger.LogInformation("AuditLogViewer: LoadAuditLogData returning {Count} items. TotalCount: {TotalCount}", result.Data.Count(), result.TotalCount);
            foreach (var item in result.Data)
            {
                _logger.LogInformation("AuditLogViewer: Item - Id: {Id}, Timestamp: {Timestamp}, ActionType: {ActionType}, EntityName: {EntityName}, PrimaryKey: {PrimaryKey}", item.Id, item.Timestamp, item.ActionType, item.EntityName, item.PrimaryKey);
            }
            return new DataResult<AuditLog>
            {
                Data = result.Data,
                TotalCount = result.TotalCount
            };
        }

        private async Task ToggleQueryBuilder()
        {
            _isQueryBuilderVisible = !_isQueryBuilderVisible;
            if (_isQueryBuilderVisible)
            {
                await LoadSavedQueries();
            }
        }

        private async Task HandleSaveQuery(DataHub.Core.Models.UI.SavedCriteria criteria)
        {
            var tableName = "AuditLog";
            await UserPreferenceService.SaveCriteriaAsync(tableName, criteria.Name, criteria.Filters, criteria.Sorts, criteria.RawQuery);
            await LoadSavedQueries(); // Refresh the list after saving
        }

        private async Task HandleDeleteQuery(string queryName)
        { 
            var tableName = "AuditLog";
            await UserPreferenceService.DeleteCriteriaAsync(tableName, queryName);
            await LoadSavedQueries(); // Refresh the list after deleting
        }

        private async Task HandleQueryApplied(DataHub.Core.Models.UI.SavedCriteria criteria) // Changed parameter type to UI.SavedCriteria
        {
            _advancedQuery = criteria.RawQuery; // Use RawQuery from UI.SavedCriteria
            _activeFilterName = criteria.Name; // Set active filter name
            // Trigger data reload through DataProvider
            if (_dataView != null)
            {
                await _dataView.RefreshDataAsync();
            }
            _isQueryBuilderVisible = false; // Close the FloatingQueryBuilder
        }

        private async Task HandleQueryClear()
        {
            _advancedQuery = string.Empty;
            _activeFilterName = null; // Clear active filter name
            if (_dataView != null)
            {
                await _dataView.RefreshDataAsync();
            }
        }

        private async Task LoadSavedQueries()
        {
            var tableName = "AuditLog";
            _savedQueries.Clear();
            var queryNames = await UserPreferenceService.ListSavedCriteriaNamesAsync(tableName);
            foreach (var name in queryNames)
            {
                var savedCriteria = await UserPreferenceService.LoadCriteriaAsync(tableName, name);
                if (savedCriteria != null)
                {
                    _savedQueries.Add(new UserSavedCriteria
                    {
                        CriteriaName = savedCriteria.Name,
                        RawQuery = savedCriteria.RawQuery,
                        TableName = tableName
                    });
                }
            }
        }

        private async Task<Stream> ExportAuditLogsToExcel(DataRequestBase request)
        {
            try
            {
                // Retrieve all data based on the current filter
                var allDataResult = await _auditLogService.GetPagedAsync(1, int.MaxValue, rawQuery: _advancedQuery);
                return await _excelExportService.ExportToExcelAsync(allDataResult.Data, columnDefinitions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting audit logs to Excel.");
                // Optionally, set an error message for the user
                return new MemoryStream(); // Return an empty stream on error
            }
        }

        private void ViewLogDetails(int logId)
        {
            // Assuming a details page exists at this route.
            // If not, this will need to be created.
            NavigationManager.NavigateTo($"/auditlog/view/{logId}");
        }
    }
}