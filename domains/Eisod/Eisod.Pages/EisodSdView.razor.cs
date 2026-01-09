using Microsoft.AspNetCore.Components;
using Eisod.Shared.Models;
using DataHub.Core.Models.UI;
using Eisod.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataHub.Core.Models.DataGrid;
using System.Linq;
using Microsoft.Extensions.Logging;
using System;
using DataHub.Core.Components.Shared;
using System.IO;
using System.Linq.Expressions;
using DataHub.Core.Services;
using DataHub.Core.Models; // For UserSavedCriteria

namespace Eisod.Pages
{
    public partial class EisodSdView
    {
        [Inject] private IViewEisodSdService _eisodSdService { get; set; } = default!;
        [Inject] private ILogger<EisodSdView> _logger { get; set; } = default!;
        [Inject] private IExcelExportService _excelExportService { get; set; } = default!;
        [Inject] private UserPreferenceService UserPreferenceService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        private GenericDataView<ViewEisodSdUIModel>? _dataView;
        private bool _isQueryBuilderVisible = false;
        private string? _advancedQuery;
        private string? _activeFilterName;
        private HashSet<ViewEisodSdUIModel> _selectedItems = new();

        private List<UserSavedCriteria> _savedQueries = new();

        private List<DataHub.Core.Models.DataGrid.ColumnDefinition<ViewEisodSdUIModel>> _columnDefinitions = new();
        private List<DataHub.Core.Models.UI.ColumnDefinition> _queryBuilderColumns = new();

        protected override async Task OnInitializedAsync()
        {
            _columnDefinitions = new List<DataHub.Core.Models.DataGrid.ColumnDefinition<ViewEisodSdUIModel>>
            {
                new() { FieldName = nameof(ViewEisodSdUIModel.Id), DisplayName = "Id", GetValue = item => item.Id ?? string.Empty, DataType = typeof(string), IsSortable = true, IsFilterable = true, IsVisible = true },
                new() { FieldName = nameof(ViewEisodSdUIModel.InterníKódVzorce), DisplayName = "Internal Code", GetValue = item => item.InterníKódVzorce, DataType = typeof(int), IsSortable = true, IsFilterable = true, IsVisible = true },
                new() { FieldName = nameof(ViewEisodSdUIModel.ČísloPoložky), DisplayName = "Číslo položky", GetValue = item => item.ČísloPoložky, DataType = typeof(string), IsSortable = true, IsFilterable = true, IsVisible = true },
                new() { FieldName = nameof(ViewEisodSdUIModel.Vzorec), DisplayName = "Vzorec", GetValue = item => item.Vzorec, DataType = typeof(string), IsSortable = true, IsFilterable = true, IsVisible = true },
                new() { FieldName = nameof(ViewEisodSdUIModel.ČísloOperace), DisplayName = "Číslo operace", GetValue = item => item.ČísloOperace ?? 0, DataType = typeof(int?), IsSortable = true, IsFilterable = true, IsVisible = true },
                new() { FieldName = nameof(ViewEisodSdUIModel.TypOperace), DisplayName = "Typ operace", GetValue = (ViewEisodSdUIModel item) => item.TypOperace ?? string.Empty, DataType = typeof(string), IsSortable = true, IsFilterable = true, IsVisible = true },
                new() { FieldName = nameof(ViewEisodSdUIModel.Hodnota), DisplayName = "Hodnota", GetValue = item => item.TruncatedHodnota ?? string.Empty, DataType = typeof(string), IsSortable = true, IsFilterable = true, IsVisible = true },
                new() { FieldName = nameof(ViewEisodSdUIModel.ČísloVzorce), DisplayName = "Číslo vzorce", GetValue = item => item.ČísloVzorce, DataType = typeof(int), IsSortable = true, IsFilterable = true, IsVisible = true },
            };

            _queryBuilderColumns = _columnDefinitions.Select(c => new DataHub.Core.Models.UI.ColumnDefinition
            {
                FieldName = c.FieldName,
                DisplayName = c.DisplayName,
                DataType = c.DataType,
                IsFilterable = c.IsFilterable,
                IsSortable = c.IsSortable
            }).ToList();

            await LoadSavedQueries();
        }

        private async Task LoadSavedQueries()
        {
            var tableName = "ViewEisodSd";
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
        
        private async Task HandleSaveQuery(SavedCriteria criteria)
        {
            var tableName = "ViewEisodSd";
            await UserPreferenceService.SaveCriteriaAsync(tableName, criteria.Name, criteria.Filters, criteria.Sorts, criteria.RawQuery);
            await LoadSavedQueries();
        }

        private async Task HandleDeleteQuery(string queryName)
        {
            var tableName = "ViewEisodSd";
            await UserPreferenceService.DeleteCriteriaAsync(tableName, queryName);
            await LoadSavedQueries();
        }

        private async Task<DataResult<ViewEisodSdUIModel>> LoadData(DataRequestBase request)
        {
            _logger.LogInformation("EisodSdView: LoadData called. Page: {Page}, PageSize: {PageSize}, RawQuery: {RawQuery}", request.Page, request.PageSize, _advancedQuery);
            var serverRequest = new DataHub.Core.Models.DataGrid.ServerDataRequest 
            { 
                Page = request.Page, 
                PageSize = request.PageSize, 
                RawQuery = _advancedQuery,
                Sorts = request.Sorts
            };
            var result = await _eisodSdService.GetPagedAsync(serverRequest);
            var mappedData = result.Data.Select(x => new ViewEisodSdUIModel
            {
                ČísloOperace = x.ČísloOperace,
                ČísloPoložky = x.ČísloPoložky,
                ČísloVzorce = x.ČísloVzorce,
                DruhOperace = x.DruhOperace,
                Hodnota = x.Hodnota,
                InterníKódVzorce = x.InterníKódVzorce,
                KeyOvv = x.KeyOvv,
                SkupinaOperace = x.SkupinaOperace,
                TypOperace = x.TypOperace,
                Vzorec = x.Vzorec,
                ZobrazitVSd = x.ZobrazitVSd
            }).ToList();

            _logger.LogInformation("EisodSdView: LoadData returning {Count} items. TotalCount: {TotalCount}", mappedData.Count, result.TotalCount);
            return new DataResult<ViewEisodSdUIModel>
            {
                Data = mappedData,
                TotalCount = result.TotalCount
            };
        }

        private async Task HandleQueryApplied(SavedCriteria criteria)
        {
            _logger.LogInformation("EisodSdView: HandleQueryApplied received raw query: {RawQuery}", criteria.RawQuery);
            _advancedQuery = criteria.RawQuery;
            _activeFilterName = criteria.Name;
            _isQueryBuilderVisible = false;
            if (_dataView != null)
            {
                await _dataView.RefreshDataAsync();
            }
        }

        private async Task HandleQueryClear()
        {
            _advancedQuery = null;
            _activeFilterName = null;
            _isQueryBuilderVisible = false;
            if (_dataView != null)
            {
                await _dataView.RefreshDataAsync();
            }
        }

        private void ToggleQueryBuilder()
        {
            _isQueryBuilderVisible = !_isQueryBuilderVisible;
        }

        private async Task<Stream> ExportEisodSdsToExcel(DataRequestBase request)
        {
            try
            {
                var serverRequest = new DataHub.Core.Models.DataGrid.ServerDataRequest 
                { 
                    Page = 1, 
                    PageSize = int.MaxValue, 
                    RawQuery = _advancedQuery, 
                    GetAll = true 
                };
                var allDataResult = await _eisodSdService.GetPagedAsync(serverRequest);
                var mappedData = allDataResult.Data.Select(x => new ViewEisodSdUIModel
                {
                    ČísloOperace = x.ČísloOperace,
                    ČísloPoložky = x.ČísloPoložky,
                    ČísloVzorce = x.ČísloVzorce,
                    DruhOperace = x.DruhOperace,
                    Hodnota = x.Hodnota,
                    InterníKódVzorce = x.InterníKódVzorce,
                    KeyOvv = x.KeyOvv,
                    SkupinaOperace = x.SkupinaOperace,
                    TypOperace = x.TypOperace,
                    Vzorec = x.Vzorec,
                    ZobrazitVSd = x.ZobrazitVSd
                }).ToList();
                return await _excelExportService.ExportToExcelAsync(mappedData, _columnDefinitions, _activeFilterName, _advancedQuery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting EISOD SD data to Excel.");
                return new MemoryStream();
            }
        }

        private void ViewItemDetails(string id)
        {
            NavigationManager.NavigateTo($"/eisodsd/view/{id}");
        }
    }
}
