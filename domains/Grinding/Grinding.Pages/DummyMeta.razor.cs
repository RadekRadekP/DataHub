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
using DummyMetaEntity = DataHub.Core.Models.DummyMeta;

namespace Grinding.Pages
{
    /// <summary>
    /// Clean metadata-driven implementation using DummyMeta entity
    /// </summary>
    public partial class DummyMeta : ComponentBase
    {
        [Inject]
        private IDummyMetaService DummyMetaService { get; set; } = default!;

        [Inject]
        private ILogger<DummyMeta> Logger { get; set; } = default!;

        [Inject]
        private IExcelExportService ExcelExportService { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        [Inject]
        private NavigationContextService NavContext { get; set; } = default!;

        private GenericDataView<DummyMetaEntity>? _dataView;
        private HashSet<DummyMetaEntity> _selectedItems = new();

        // Temporary hardcoded columns for testing (until metadata discovery works)
        private List<ColumnDefinition<DummyMetaEntity>> _columns = new()
        {
            new ColumnDefinition<DummyMetaEntity> { FieldName = "Id", DisplayName = "ID", GetValue = x => x.Id, IsVisible = true },
            new ColumnDefinition<DummyMetaEntity> { FieldName = "Name", DisplayName = "Name", GetValue = x => x.Name, IsVisible = true },
            new ColumnDefinition<DummyMetaEntity> { FieldName = "Description", DisplayName = "Description", GetValue = x => x.Description ?? string.Empty, IsVisible = true },
            new ColumnDefinition<DummyMetaEntity> { FieldName = "CustomTag", DisplayName = "Tag", GetValue = x => x.CustomTag ?? string.Empty, IsVisible = true },
            new ColumnDefinition<DummyMetaEntity> { FieldName = "CategoryId", DisplayName = "Category", GetValue = x => x.CategoryId, IsVisible = true },
            new ColumnDefinition<DummyMetaEntity> { FieldName = "StatusId", DisplayName = "Status", GetValue = x => x.StatusId, IsVisible = true },
            new ColumnDefinition<DummyMetaEntity> { FieldName = "Value", DisplayName = "Value", GetValue = x => x.Value, IsVisible = true },
            new ColumnDefinition<DummyMetaEntity> { FieldName = "CreatedDate", DisplayName = "Created", GetValue = x => x.CreatedDate, IsVisible = true },
            new ColumnDefinition<DummyMetaEntity> { FieldName = "IsActive", DisplayName = "Active", GetValue = x => x.IsActive, IsVisible = true },
        };

        // 🎯 Note: Columns above are temporary for testing
        // Will be replaced with metadata-driven columns (MetaEntityId) later

        private async Task<DataResult<DummyMetaEntity>> LoadData(DataRequestBase requestBase)
        {
            Logger.LogInformation("DummyMeta.LoadData called - Page: {Page}, PageSize: {PageSize}", requestBase.Page, requestBase.PageSize);

            var serverDataRequest = new ServerDataRequest
            {
                Page = requestBase.Page,
                PageSize = requestBase.PageSize,
                Sorts = requestBase.Sorts,
                GetAll = requestBase.GetAll
            };
            
            var serverDataResult = await DummyMetaService.GetPagedAsync(serverDataRequest);

            Logger.LogInformation("DummyMeta.LoadData returning {Count} items, TotalCount: {TotalCount}", 
                serverDataResult.Data.Count(), serverDataResult.TotalCount);

            return new DataResult<DummyMetaEntity>
            {
                Data = serverDataResult.Data,
                TotalCount = serverDataResult.TotalCount
            };
        }

        private async Task<Stream> ExportDataToExcel(DataRequestBase request)
        {
            // Temporary basic columns for export
            var tempColumns = new List<ColumnDefinition<DummyMetaEntity>>
            {
                new ColumnDefinition<DummyMetaEntity> { FieldName = "Id", DisplayName = "ID", GetValue = x => x.Id },
                new ColumnDefinition<DummyMetaEntity> { FieldName = "Name", DisplayName = "Name", GetValue = x => x.Name },
                new ColumnDefinition<DummyMetaEntity> { FieldName = "Description", DisplayName = "Description", GetValue = x =>  x.Description ?? string.Empty }
            };

            var serverDataRequest = new ServerDataRequest
            {
                Page = 1,
                PageSize = int.MaxValue,
                GetAll = true
            };
            
            var result = await DummyMetaService.GetPagedAsync(serverDataRequest);
            return await ExcelExportService.ExportToExcelAsync(result.Data.ToList(), tempColumns, "DummyMeta_Export", null);
        }

        private void ViewItem(int id)
        {
            Logger.LogInformation("View DummyMeta item: {Id}", id);
            NavContext.SetNavigationContext(new List<object> { id }, "/dummymeta/action", RecordInteractionMode.View);
            NavigationManager.NavigateTo($"/dummymeta/action/{id}");
        }

        private void EditItem(int id)
        {
            Logger.LogInformation("Edit DummyMeta item: {Id}", id);
            NavContext.SetNavigationContext(new List<object> { id }, "/dummymeta/action", RecordInteractionMode.Edit);
            NavigationManager.NavigateTo($"/dummymeta/action/{id}");
        }

        private async Task DeleteItem(int id)
        {
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to delete DummyMeta item with ID: {id}?");
            if (confirmed)
            {
                await DummyMetaService.DeleteAsync(id);
                _selectedItems.RemoveWhere(i => i.Id == id);
                if (_dataView is not null) await _dataView.RefreshDataAsync();
                Logger.LogInformation("Deleted DummyMeta item: {Id}", id);
            }
        }

        private void ViewSelectedItems()
        {
            if (!_selectedItems.Any())
            {
                Logger.LogWarning("No items selected for viewing.");
                return;
            }

            var selectedIds = _selectedItems.Select(i => (object)i.Id).ToList();
            Logger.LogInformation("Viewing {Count} selected DummyMeta items", selectedIds.Count);
            NavContext.SetNavigationContext(selectedIds, "/dummymeta/action", RecordInteractionMode.View);
            NavigationManager.NavigateTo($"/dummymeta/action/{selectedIds.First()}");
        }

        private void EditSelectedItems()
        {
            if (!_selectedItems.Any())
            {
                Logger.LogWarning("No items selected for editing.");
                return;
            }

            var selectedIds = _selectedItems.Select(i => (object)i.Id).ToList();
            Logger.LogInformation("Editing {Count} selected DummyMeta items", selectedIds.Count);
            NavContext.SetNavigationContext(selectedIds, "/dummymeta/action", RecordInteractionMode.Edit);
            NavigationManager.NavigateTo($"/dummymeta/action/{selectedIds.First()}");
        }

        private async Task DeleteSelectedItems()
        {
            if (!_selectedItems.Any())
            {
                Logger.LogWarning("No items selected for deletion.");
                return;
            }

            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to delete {_selectedItems.Count} selected items?");
            if (confirmed)
            {
                foreach (var item in _selectedItems.ToList())
                {
                    await DummyMetaService.DeleteAsync(item.Id);
                }
                _selectedItems.Clear();
                if (_dataView is not null) await _dataView.RefreshDataAsync();
                Logger.LogInformation("Deleted {Count} selected DummyMeta items", _selectedItems.Count);
            }
        }
    }
}
