using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using DataHub.Core.Components.Shared;
using Grinding.Shared.Models;
using DataHub.Core.Models.DataGrid;
using DataHub.Core.Models.UI;
using Grinding.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataHub.Core.Models.Interfaces;
using DataHub.Core.Services;
using DataHub.Core.Models; // For UserSavedCriteria
using DataHub.Core.Interfaces; // For IDataService

namespace Grinding.Pages
{
    public partial class Alarms : ComponentBase
    {
        [Inject]
        private IDataService<Alarm> AlarmDataService { get; set; } = default!;

        [Inject]
        private IAlarmSpecificService AlarmSpecificService { get; set; } = default!;

        [Inject]
        private ILogger<Alarms> Logger { get; set; } = default!;

        [Inject]
        private UserPreferenceService UserPreferenceService { get; set; } = default!;

        [Inject]
        private IExcelExportService ExcelExportService { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        [Inject]
        private NavigationContextService NavContext { get; set; } = default!;

        [Inject]
        private AutoMapper.IMapper Mapper { get; set; } = default!;

        private GenericDataView<AlarmUIModel>? _dataView;
        private string? errorMessage;
        private string? infoMessage;
        private List<ColumnDefinition<AlarmUIModel>> _columnDefinitions = new();
        private List<FilterCriterion> _currentFilters = new();
        private List<SortCriterion> _currentSortCriteria = new();
        private HashSet<AlarmUIModel> _selectedAlarms = new();
        private string _advancedQuery = string.Empty;
        private string? _activeFilterName;
        private bool _isQueryBuilderVisible = false;
        private List<DataHub.Core.Models.UI.ColumnDefinition> _queryBuilderColumns = new();

        private List<UserSavedCriteria> _savedQueries = new();

        protected override async Task OnInitializedAsync()
        {
            _columnDefinitions = new List<ColumnDefinition<AlarmUIModel>>
            {
                new() { FieldName = nameof(AlarmUIModel.Id), DisplayName = "ID", IsSortable = true, IsFilterable = true, DataType = typeof(int), GetValue = item => item.Id, IsVisible = true },
                new() { FieldName = nameof(AlarmUIModel.ClientDbId), DisplayName = "Client DB ID", IsSortable = true, IsFilterable = true, DataType = typeof(int), GetValue = item => item.ClientDbId, IsVisible = true },
                new() { FieldName = nameof(AlarmUIModel.ClientId), DisplayName = "Client ID", IsSortable = true, IsFilterable = true, DataType = typeof(string), GetValue = item => item.ClientId, IsVisible = true },
                new() { FieldName = nameof(AlarmUIModel.AlarmDate), DisplayName = "Alarm Date", IsSortable = true, IsFilterable = true, DataType = typeof(DateTime), GetValue = item => item.AlarmDate, IsVisible = true },
                new() { FieldName = nameof(AlarmUIModel.Type), DisplayName = "Type", IsSortable = true, IsFilterable = true, DataType = typeof(string), GetValue = item => item.Type, IsVisible = true },
                new() { FieldName = nameof(AlarmUIModel.Nr), DisplayName = "Nr", IsSortable = true, IsFilterable = true, DataType = typeof(int), GetValue = item => item.Nr, IsVisible = true },
                new() { FieldName = nameof(AlarmUIModel.Text), DisplayName = "Text", IsSortable = true, IsFilterable = true, DataType = typeof(string), GetValue = item => item.Text, IsVisible = true },
                new() { FieldName = nameof(AlarmUIModel.Operator), DisplayName = "Operator", IsSortable = true, IsFilterable = true, DataType = typeof(string), GetValue = item => item.Operator, IsVisible = true },
                new() { FieldName = nameof(AlarmUIModel.ServerTimestamp), DisplayName = "Server Timestamp", IsSortable = true, IsFilterable = true, DataType = typeof(DateTime), GetValue = item => item.ServerTimestamp, IsVisible = true },
                new() { FieldName = nameof(AlarmUIModel.ChangeCounter), DisplayName = "Change Counter", IsSortable = true, IsFilterable = true, DataType = typeof(int), GetValue = item => item.ChangeCounter, IsVisible = true },
            };
            _queryBuilderColumns = _columnDefinitions.Select(cd => new DataHub.Core.Models.UI.ColumnDefinition
            {
                FieldName = cd.FieldName,
                DisplayName = cd.DisplayName
            }).ToList();
            await LoadSavedQueries();
        }

        async Task LoadSavedQueries()
        {
            var tableName = "Alarms";
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

        async Task HandleSaveQuery(DataHub.Core.Models.UI.SavedCriteria criteria)
        {
            var tableName = "Alarms";
            await UserPreferenceService.SaveCriteriaAsync(tableName, criteria.Name, criteria.Filters, criteria.Sorts, criteria.RawQuery);
            await LoadSavedQueries();
        }

        async Task HandleDeleteQuery(string queryName)
        {
            var tableName = "Alarms";
            await UserPreferenceService.DeleteCriteriaAsync(tableName, queryName);
            await LoadSavedQueries();
        }

        async Task<DataResult<AlarmUIModel>> LoadAlarmsData(DataRequestBase dataRequestBase)
        {
            errorMessage = null;
            try
            {
                var serverDataRequest = new ServerDataRequest
                {
                    Page = dataRequestBase.Page,
                    PageSize = dataRequestBase.PageSize,
                    RawQuery = _advancedQuery,
                    Sorts = dataRequestBase.Sorts,
                    GetAll = dataRequestBase.GetAll
                };

                Logger.LogInformation("Alarms page: Calling DataProvider with Page={Page}, PageSize={PageSize}, RawQuery={RawQuery}", serverDataRequest.Page, serverDataRequest.PageSize, serverDataRequest.RawQuery);

                var serverDataResult = await AlarmDataService.GetPagedAsync(serverDataRequest);

                var mappedData = Mapper.Map<List<AlarmUIModel>>(serverDataResult.Data);

                return new DataResult<AlarmUIModel>
                {
                    Data = mappedData,
                    TotalCount = serverDataResult.TotalCount
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading alarm data.");
                errorMessage = "An error occurred while loading data. Please try again.";
                return new DataResult<AlarmUIModel> { Data = new List<AlarmUIModel>(), TotalCount = 0, TotalUnfilteredCount = 0 };
            }
        }

        async Task<Stream> ExportAlarms(DataRequestBase request)
        {
            try
            {
                return await AlarmSpecificService.ExportAlarmsToExcelAsync(request, _activeFilterName, _advancedQuery, _currentFilters, _currentSortCriteria);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error preparing data for Excel export.");
                errorMessage = "Failed to export data.";
                return new MemoryStream();
            }
        }

        async Task OnAdvancedQueryApplied(SavedCriteria criteria)
        {
            Logger.LogInformation("Alarms: OnAdvancedQueryApplied - Filters count: {FilterCount}, Sorts count: {SortsCount}", criteria.Filters.Count, criteria.Sorts.Count);
            Logger.LogInformation("Alarms: OnAdvancedQueryApplied - RawQuery received: {RawQuery}", criteria.RawQuery);

            _currentFilters = criteria.Filters;
            _currentSortCriteria = criteria.Sorts;
            _advancedQuery = criteria.RawQuery;
            _activeFilterName = criteria.Name;
            errorMessage = null;
            infoMessage = null;
            await _dataView!.RefreshDataAsync();
        }

        

        async Task OnAdvancedQueryClear()
        {
            _advancedQuery = string.Empty;
            _activeFilterName = null;
            _currentFilters.Clear();
            _currentSortCriteria.Clear();
            errorMessage = null;
            infoMessage = null;
            if (_dataView != null)
            {
                await _dataView.RefreshDataAsync();
            }
        }

        void ToggleQueryBuilder()
        {
            _isQueryBuilderVisible = !_isQueryBuilderVisible;
        }

        private void AddNewAlarm()
        {
            NavContext.SetNavigationContext(new List<object> { 0 }, "/alarms/action", RecordInteractionMode.Add);
            NavigationManager.NavigateTo("/alarms/action/0");
        }

        private void ViewSelectedAlarms()
        {
            if (_selectedAlarms.Any())
            {
                var selectedIds = _selectedAlarms.Select(a => (object)a.Id).ToList();
                NavContext.SetNavigationContext(selectedIds, "/alarms/action", RecordInteractionMode.View);
                var firstAlarmId = _selectedAlarms.First().Id;
                NavigationManager.NavigateTo($"/alarms/action/{firstAlarmId}");
            }
        }

        private void CopySelectedAlarms()
        {
            if (_selectedAlarms.Any())
            {
                var selectedIds = _selectedAlarms.Select(a => (object)a.Id).ToList();
                NavContext.SetNavigationContext(selectedIds, "/alarms/action", RecordInteractionMode.Copy);
                var firstAlarmId = _selectedAlarms.First().Id;
                NavigationManager.NavigateTo($"/alarms/action/{firstAlarmId}");
            }
        }

        private void ViewAlarm(int alarmId)
        {
            NavContext.SetNavigationContext(new List<object> { alarmId }, "/alarms/action", RecordInteractionMode.View);
            NavigationManager.NavigateTo($"/alarms/action/{alarmId}");
        }

        private void EditAlarm(int alarmId)
        {
            NavContext.SetNavigationContext(new List<object> { alarmId }, "/alarms/action", RecordInteractionMode.Edit);
            NavigationManager.NavigateTo($"/alarms/action/{alarmId}");
        }

        private void DeleteAlarm(int alarmId)
        {
            NavContext.SetNavigationContext(new List<object> { alarmId }, "/alarms/action", RecordInteractionMode.Delete);
            NavigationManager.NavigateTo($"/alarms/action/{alarmId}");
        }

        private void CopyAlarm(int alarmId)
        {
            NavContext.SetNavigationContext(new List<object> { alarmId }, "/alarms/action", RecordInteractionMode.Copy);
            NavigationManager.NavigateTo($"/alarms/action/{alarmId}");
        }

        private void EditSelectedAlarms()
        {
            if (_selectedAlarms.Any())
            {
                var selectedIds = _selectedAlarms.Select(a => (object)a.Id).ToList();
                NavContext.SetNavigationContext(selectedIds, "/alarms/action", RecordInteractionMode.Edit);
                var firstAlarmId = _selectedAlarms.First().Id;
                NavigationManager.NavigateTo($"/alarms/action/{firstAlarmId}");
            }
        }

        private void DeleteSelectedAlarms()
        {
            var alarmIds = _selectedAlarms.Select(a => (object)a.Id).ToList();
            if (!alarmIds.Any())
            {
                infoMessage = "No alarms selected for deletion.";
                return;
            }
            NavContext.SetNavigationContext(alarmIds, "/alarms/action", RecordInteractionMode.Delete);
            var firstAlarmId = _selectedAlarms.First().Id;
            NavigationManager.NavigateTo($"/alarms/action/{firstAlarmId}");
        }
    }
}