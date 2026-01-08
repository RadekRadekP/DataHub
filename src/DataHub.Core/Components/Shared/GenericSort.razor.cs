using Microsoft.AspNetCore.Components;
using DataHub.Core.Models.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataHub.Core.Models.DataGrid; // Add this line

namespace DataHub.Core.Components.Shared
{
    public partial class GenericSort<TItem> where TItem : class
    {
        [Parameter] public List<ColumnDefinition<TItem>> ColumnDefinitions { get; set; } = new();
        [Parameter] public EventCallback<List<SortCriterion>> OnSortApplied { get; set; } // Change to List<SortCriterion>

        private List<SortCriterion> _sortCriteria = new(); // Change to list

        protected override void OnInitialized()
        {
            // Initialize with one sort criterion by default
            AddSortRow();
        }

        private void AddSortRow()
        {
            if (_sortCriteria.Count < 3) // Limit to 3 sorting fields
            {
                _sortCriteria.Add(new SortCriterion
                {
                    FieldName = ColumnDefinitions.FirstOrDefault(c => c.IsSortable)?.FieldName ?? string.Empty,
                    SortDirection = "asc"
                });
            }
        }

        private void RemoveSortRow(SortCriterion criterion)
        {
            _sortCriteria.Remove(criterion);
            if (!_sortCriteria.Any()) // Ensure at least one row remains
            {
                AddSortRow();
            }
        }

        private async Task ApplySort()
        {
            // Filter out any empty sort criteria before applying
            var validSorts = _sortCriteria.Where(s => !string.IsNullOrEmpty(s.FieldName)).ToList();
            await OnSortApplied.InvokeAsync(validSorts);
        }

        private async Task ClearSort()
        {
            _sortCriteria.Clear();
            AddSortRow(); // Reset to one empty row
            await OnSortApplied.InvokeAsync(new List<SortCriterion>()); // Indicate no sorting
        }
    }
}