using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using RPK_BlazorApp.Models.UI;
using RPK_BlazorApp.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RPK_BlazorApp.Models.DataGrid;

namespace RPK_BlazorApp.Components.Shared
{
    public partial class AdvancedFilterBuilder<TItem> : ComponentBase where TItem : class
    {
        [Inject] public QueryParserService QueryParser { get; set; } = default!;
        [Inject] public UserPreferenceService UserPreferenceService { get; set; } = default!;
        [Inject] public ILogger<AdvancedFilterBuilder<TItem>> Logger { get; set; } = default!;

        [Parameter] public string CurrentQuery { get; set; } = string.Empty;
        [Parameter] public EventCallback<string> OnQueryChanged { get; set; }
        [Parameter] public EventCallback<SavedCriteria> OnQueryApplied { get; set; }

        [Parameter] public List<Models.DataGrid.ColumnDefinition<TItem>> AvailableColumns { get; set; } = new();
        [Parameter] public string TableName { get; set; } = string.Empty;

        private bool showQueryBuilderModal = false;
        private List<SavedCriteria> savedQueries = new();
        private string currentQueryName = string.Empty; // To hold the name of the query being edited/saved

        private async Task OpenQueryBuilderModal()
        {
            await LoadSavedQueries();
            showQueryBuilderModal = true;
        }

        private void CloseQueryBuilderModal()
        {
            showQueryBuilderModal = false;
            // Optionally, reset QueryName or other state here
        }

        private async Task HandleQueryBuilderSave(SavedCriteria savedCriteria)
        {
            Logger.LogInformation("DEBUG: HandleQueryBuilderSave - Name: '{Name}', RawQuery: '{RawQuery}'", savedCriteria.Name, savedCriteria.RawQuery); // Added debug log
            if (string.IsNullOrWhiteSpace(savedCriteria.Name) || string.IsNullOrWhiteSpace(savedCriteria.RawQuery))
            {
                // TODO: Show validation message in modal
                return;
            }

            try
            {
                var parsedCriteria = QueryParser.ParseQuery(savedCriteria.RawQuery);
                await UserPreferenceService.SaveCriteriaAsync(TableName, savedCriteria.Name, parsedCriteria.Filters, parsedCriteria.Sorts, savedCriteria.RawQuery);
                Logger.LogInformation("Criteria '{Name}' for table '{TableName}' saved successfully.", savedCriteria.Name, TableName);
                await LoadSavedQueries();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving criteria '{Name}' for table '{TableName}'.", savedCriteria.Name, TableName);
                errorMessage = $"Error saving criteria: {ex.Message}";
            }
        }

        private async Task HandleQueryApplied(SavedCriteria criteria)
        {
            CurrentQuery = criteria.RawQuery;
            currentQueryName = criteria.Name;
            await OnQueryChanged.InvokeAsync(CurrentQuery);
            await ApplyQuery();
            showQueryBuilderModal = false;
            StateHasChanged(); // Explicitly request re-render
        }

        private async Task HandleQueryClear()
        {
            currentQueryName = string.Empty;
            CurrentQuery = string.Empty;
            await OnQueryChanged.InvokeAsync(CurrentQuery);
            await ApplyQuery();
            showQueryBuilderModal = false;
        }

        private async Task HandleQueryBuilderDelete(string queryName)
        {
            if (string.IsNullOrWhiteSpace(queryName))
            {
                // TODO: Show validation message in modal
                return;
            }

            try
            {
                await UserPreferenceService.DeleteCriteriaAsync(TableName, queryName);
                Logger.LogInformation("Criteria '{Name}' for table '{TableName}' deleted successfully.", queryName, TableName);

                if (currentQueryName == queryName)
                {
                   await HandleQueryClear();
                }
                
                await LoadSavedQueries(); // Refresh the list
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting criteria '{Name}' for table '{TableName}'.", queryName, TableName);
                errorMessage = $"Error deleting criteria: {ex.Message}";
            }
        }

        private async Task LoadSavedQueries()
        {
            try
            {
                var queryNames = await UserPreferenceService.ListSavedCriteriaNamesAsync(TableName);
                savedQueries.Clear();
                foreach (var name in queryNames)
                {
                    var criteria = await UserPreferenceService.LoadCriteriaAsync(TableName, name);
                    if (criteria != null)
                    {
                        savedQueries.Add(criteria);
                    }
                }
                Logger.LogInformation("Loaded {Count} saved queries for table '{TableName}'.", savedQueries.Count, TableName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading saved queries for table '{TableName}'.", TableName);
                errorMessage = "Could not load saved queries.";
            }
        }

        private string? errorMessage;

        private async Task ApplyQuery()
        {
            errorMessage = null; // Clear previous errors
            try
            {
                Console.WriteLine($"DEBUG: AdvancedFilterBuilder - CurrentQuery before parsing: '{CurrentQuery}'"); // Added debug line
                var criteria = QueryParser.ParseQuery(CurrentQuery);
                criteria.RawQuery = CurrentQuery;
                await OnQueryApplied.InvokeAsync(criteria);
            }
            catch (Exception ex)
            {
                errorMessage = $"Error parsing query: {ex.Message}";
                // Log the error for debugging purposes
                Console.WriteLine($"ERROR: AdvancedFilterBuilder - {errorMessage}");
            }
        }

        private async Task ClearFilter()
        {
            currentQueryName = string.Empty;
            CurrentQuery = string.Empty;
            await OnQueryChanged.InvokeAsync(CurrentQuery);
            await ApplyQuery(); // Re-apply empty query to refresh data
        }

        
    }
}