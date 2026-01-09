using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using DataHub.Core.Models;
using DataHub.Core.Models.DataGrid;
using DataHub.Core.Models.UI;
using DataHub.Core.Services;
using DataHub.Core.Components.Shared;
using Grinding.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Grinding.Pages
{
    public partial class DummyItems : ComponentBase
    {
        [Inject]
        private IDummyItemService DummyItemService { get; set; } = default!;

        [Inject]
        private ILogger<DummyItems> Logger { get; set; } = default!;

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
        private ChangeSetService ChangeSetService { get; set; } = default!;

        [Inject]
        private AutoMapper.IMapper Mapper { get; set; } = default!;

        private async Task<Stream> ExportDataToExcel(DataRequestBase request)
        {
            // Fetch all data based on current filters, ignoring pagination
            var serverDataRequest = new ServerDataRequest
            {
                Page = 1,
                PageSize = 1,
                RawQuery = _rawQuery,
                GetAll = true
            };
            var result = await DummyItemService.GetPagedAsync(serverDataRequest);
            
            // Use the ExcelExportService to create the Excel stream
            return await ExcelExportService.ExportToExcelAsync(result.Data.ToList(), _columnDefinitions, _activeFilterName, _rawQuery);
        }

        private GenericDataView<DummyItem>? _dataView;
        private bool _isQueryBuilderVisible = false;
        private string? _rawQuery;
        private string? _activeFilterName;
        private HashSet<DummyItem> _selectedItems = new();

        private List<UserSavedCriteria> _savedQueries = new();

        // This list is for the GenericDataView
        private List<DataHub.Core.Models.DataGrid.ColumnDefinition<DummyItem>> _columnDefinitions = new List<DataHub.Core.Models.DataGrid.ColumnDefinition<DummyItem>>
        {
            new DataHub.Core.Models.DataGrid.ColumnDefinition<DummyItem> { FieldName = nameof(DummyItem.Id), DisplayName = "ID", IsSortable = true, GetValue = item => item.Id },
            new DataHub.Core.Models.DataGrid.ColumnDefinition<DummyItem> { FieldName = nameof(DummyItem.Name), DisplayName = "Name", IsSortable = true, GetValue = item => item.Name },
            new DataHub.Core.Models.DataGrid.ColumnDefinition<DummyItem> { FieldName = nameof(DummyItem.Category), DisplayName = "Category", IsSortable = true, GetValue = item => item.Category ?? string.Empty },
            new DataHub.Core.Models.DataGrid.ColumnDefinition<DummyItem> { FieldName = nameof(DummyItem.CreatedDate), DisplayName = "Created Date", IsSortable = true, GetValue = item => item.CreatedDate },
            new DataHub.Core.Models.DataGrid.ColumnDefinition<DummyItem> { FieldName = nameof(DummyItem.IsActive), DisplayName = "Is Active", IsSortable = true, GetValue = item => item.IsActive }
        };

        // This list is for the FloatingQueryBuilder
        private List<DataHub.Core.Models.UI.ColumnDefinition> _queryBuilderColumns = new List<DataHub.Core.Models.UI.ColumnDefinition>();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            // Populate the separate list for the query builder
            _queryBuilderColumns = _columnDefinitions.Select(c => new DataHub.Core.Models.UI.ColumnDefinition
            {
                FieldName = c.FieldName,
                DisplayName = c.DisplayName
            }).ToList();

            await LoadSavedQueries();
        }

        private async Task LoadSavedQueries()
        {
            var tableName = "DummyItems";
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
                        TableName = tableName // Assuming TableName is part of UserSavedCriteria
                    });
                }
            }
        }

        private async Task HandleSaveQuery(DataHub.Core.Models.UI.SavedCriteria criteria)
        {
            var tableName = "DummyItems";
            await UserPreferenceService.SaveCriteriaAsync(tableName, criteria.Name, criteria.Filters, criteria.Sorts, criteria.RawQuery);
            await LoadSavedQueries(); // Refresh the list after saving
        }

        private async Task HandleDeleteQuery(string queryName)
        {
            var tableName = "DummyItems";
            await UserPreferenceService.DeleteCriteriaAsync(tableName, queryName);
            await LoadSavedQueries(); // Refresh the list after deleting
        }

        private async Task<DataResult<DummyItem>> LoadData(DataRequestBase requestBase)
        {
            var serverDataRequest = new ServerDataRequest
            {
                Page = requestBase.Page,
                PageSize = requestBase.PageSize,
                RawQuery = _rawQuery,
                Sorts = requestBase.Sorts,
                GetAll = requestBase.GetAll
            };
            var serverDataResult = await DummyItemService.GetPagedAsync(serverDataRequest);

            var mappedData = Mapper.Map<List<DummyItem>>(serverDataResult.Data);

            Logger.LogInformation("DummyItems: LoadData returning {Count} items with TotalCount {TotalCount}.", mappedData.Count(), serverDataResult.TotalCount);
            return new DataResult<DummyItem>
            {
                Data = mappedData,
                TotalCount = serverDataResult.TotalCount
            };
        }

        private async Task HandleQueryApplied(DataHub.Core.Models.UI.SavedCriteria criteria)
        {
            _rawQuery = criteria.RawQuery;
            _activeFilterName = criteria.Name; // Set active filter name
            _isQueryBuilderVisible = false;
            if (_dataView != null)
            {
                await _dataView.RefreshDataAsync();
            }
        }

        private async Task HandleQueryClear()
        {
            _rawQuery = null;
            _activeFilterName = null; // Clear active filter name
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

        private void AddNewItem()
        {
            NavigationManager.NavigateTo("/dummyitems/action/0");
        }

        private void ViewItem(int id)
        {
            NavigationManager.NavigateTo($"/dummyitems/view/{id}"); // Assuming a view page exists or will be created.
        }

        private void EditItem(int id)
        {
            NavigationManager.NavigateTo($"/dummyitems/action/{id}");
        }

        private async Task DeleteItem(int id)
        {
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to delete item with ID: {id}?");
            if (confirmed)
            {
                await DummyItemService.DeleteAsync(id);
                _selectedItems.RemoveWhere(i => i.Id == id);
                if (_dataView is not null) await _dataView.RefreshDataAsync();
            }
        }

        private void EditSelectedItems()
        {
            if (_selectedItems.Any())
            {
                // Set the navigation context before navigating
                var selectedIds = _selectedItems.Select(i => (object)i.Id).ToList();
                NavContext.SetNavigationContext(selectedIds, "/dummyitems/action", RecordInteractionMode.Edit);
                Logger.LogInformation("EditSelectedItems: NavContext set with {Count} IDs. First ID: {FirstId}", selectedIds.Count, selectedIds.FirstOrDefault());

                var firstItemId = _selectedItems.First().Id;
                NavigationManager.NavigateTo($"/dummyitems/action/{firstItemId}");
            }
        }

        private void ViewSelectedItems()
        {
            if (_selectedItems.Any())
            {
                var selectedIds = _selectedItems.Select(i => (object)i.Id).ToList();
                NavContext.SetNavigationContext(selectedIds, "/dummyitems/action", RecordInteractionMode.View);
                Logger.LogInformation("ViewSelectedItems: NavContext set with {Count} IDs. First ID: {FirstId}", selectedIds.Count, selectedIds.FirstOrDefault());

                var firstItemId = _selectedItems.First().Id;
                NavigationManager.NavigateTo($"/dummyitems/action/{firstItemId}");
            }
        }

        private void CopySelectedItems()
        {
            if (_selectedItems.Any())
            {
                var selectedIds = _selectedItems.Select(i => (object)i.Id).ToList();
                NavContext.SetNavigationContext(selectedIds, "/dummyitems/action", RecordInteractionMode.Copy);
                Logger.LogInformation("CopySelectedItems: NavContext set with {Count} IDs. First ID: {FirstId}", selectedIds.Count, selectedIds.FirstOrDefault());

                var firstItemId = _selectedItems.First().Id;
                NavigationManager.NavigateTo($"/dummyitems/action/{firstItemId}");
            }
        }

        private void DeleteSelectedItems()
        {
            if (_selectedItems.Any())
            {
                var selectedIds = _selectedItems.Select(i => (object)i.Id).ToList();
                NavContext.SetNavigationContext(selectedIds, "/dummyitems/action", RecordInteractionMode.Delete);
                Logger.LogInformation("DeleteSelectedItems: NavContext set with {Count} IDs. First ID: {FirstId}", selectedIds.Count, selectedIds.FirstOrDefault());

                var firstItemId = _selectedItems.First().Id;
                NavigationManager.NavigateTo($"/dummyitems/action/{firstItemId}");
            }
        }
    }
}