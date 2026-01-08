using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using RPK_BlazorApp.Models.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.FluentUI.AspNetCore.Components;
using RPK_BlazorApp.Models.DataGrid;

namespace RPK_BlazorApp.Components.Shared
{
    public partial class QueryBuilderModal<TItem> : ComponentBase where TItem : class
    {
        [Inject] private ILogger<QueryBuilderModal<TItem>> Logger { get; set; } = default!;

        [Parameter]
        public bool IsVisible { get; set; }

        [Parameter]
        public EventCallback<bool> IsVisibleChanged { get; set; }

        public bool IsHidden { get; set; }

        [Parameter]
        public List<Models.DataGrid.ColumnDefinition<TItem>> AvailableColumns { get; set; } = new List<Models.DataGrid.ColumnDefinition<TItem>>();

        [Parameter]
        public List<SavedCriteria> SavedQueries { get; set; } = new();

        private void LoadQuery(SavedCriteria criteria)
        {
            QueryNameValue = criteria.Name;
            QueryExpressionValue = criteria.RawQuery;
        }

        

        protected override void OnParametersSet()
        {
            IsHidden = !IsVisible;
        }

        [Parameter]
        public EventCallback<SavedCriteria> OnSave { get; set; }

        [Parameter]
        public EventCallback<string> OnDelete { get; set; }

        [Parameter]
        public EventCallback<SavedCriteria> OnApply { get; set; }

        [Parameter]
        public EventCallback OnClear { get; set; }

        [Parameter]
        public EventCallback OnClose { get; set; }

        public string QueryNameValue { get; set; } = string.Empty;
        public string QueryExpressionValue { get; set; } = string.Empty;

        private List<string> OperatorsAndHelpers = new List<string>
        {
            "EQ", "NEQ", "GT", "GTE", "LT", "LTE",
            "CONTAINS", "STARTSWITH", "ENDSWITH",
            "IN", "NOTIN", "ISNULL", "ISNOTNULL",
            "ISTRUE", "ISFALSE",
            "(", ")", ",", "'", "AND", "OR"
        };

        public void InsertText(string text)
        {
            QueryExpressionValue += text + " ";
        }

        public async Task SaveQuery()
        {
            Logger.LogInformation("DEBUG: QueryBuilderModal.SaveQuery - QueryNameValue: '{QueryNameValue}', QueryExpressionValue: '{QueryExpressionValue}'", QueryNameValue, QueryExpressionValue);
            var savedCriteria = new SavedCriteria
            {
                Name = QueryNameValue,
                RawQuery = QueryExpressionValue
            };
            await OnSave.InvokeAsync(savedCriteria);
        }

        public async Task DeleteQuery()
        {
            await OnDelete.InvokeAsync(QueryNameValue);
        }

        private async Task ApplyCurrentQuery()
        {
            var criteria = new SavedCriteria
            {
                Name = QueryNameValue,
                RawQuery = QueryExpressionValue
            };
            await OnApply.InvokeAsync(criteria);
        }

        private async Task ClearCurrentQuery()
        {
            QueryNameValue = string.Empty;
            QueryExpressionValue = string.Empty;
            await OnClear.InvokeAsync();
        }

        public async Task CloseModal()
        {
            await OnClose.InvokeAsync();
            await IsVisibleChanged.InvokeAsync(false);
        }
    }
}