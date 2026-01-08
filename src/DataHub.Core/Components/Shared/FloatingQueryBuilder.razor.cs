using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using DataHub.Core.Models;
using DataHub.Core.Models.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.FluentUI.AspNetCore.Components; // Add this line

namespace DataHub.Core.Components.Shared
{
    public partial class FloatingQueryBuilder<TItem> : ComponentBase, IAsyncDisposable where TItem : class
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        private ElementReference draggableContainer;
        private ElementReference draggableHeader;
        private FluentTextField? queryNameField;

        [Parameter] public bool Visible { get; set; }
        [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
        [Parameter] public List<ColumnDefinition> AvailableColumns { get; set; } = new List<ColumnDefinition>();
        [Parameter] public List<UserSavedCriteria> SavedQueries { get; set; } = new List<UserSavedCriteria>();
        [Parameter] public EventCallback<SavedCriteria> OnSave { get; set; } // Reverted type
        [Parameter] public EventCallback<string> OnDelete { get; set; }
        [Parameter] public EventCallback<SavedCriteria> OnApply { get; set; } // Reverted type
        [Parameter] public EventCallback OnClear { get; set; }

        private string QueryNameValue { get; set; } = "";
        private string QueryExpressionValue { get; set; } = "";
        private string TestFieldValue { get; set; } = "";

        private List<string> OperatorsAndHelpers = new List<string>
        {
            // Filter
            "EQ", "NEQ", "GT", "LT", "GTE", "LTE",
            "Contains", "StartsWith", "EndsWith",
            "IN", "NOTIN", "ISNULL", "ISNOTNULL", "ISTRUE", "ISFALSE",

            // Logical
            "AND", "OR",

            // Order
            "ORDERBY", "ASC", "DESC",

            // Constants
            "()"
        };

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (Visible)
            {
                await JSRuntime.InvokeVoidAsync("window.makeDraggable", draggableContainer, draggableHeader);
            }
        }

        private async Task Close()
        {
            Visible = false;
            await VisibleChanged.InvokeAsync(Visible);
        }

        #pragma warning disable CS8601
        private void LoadQuery(UserSavedCriteria criteria)
        {
            QueryNameValue = criteria.CriteriaName;
            StateHasChanged(); // Force UI update for QueryNameValue
            if(criteria.RawQuery is not null)
                QueryExpressionValue = criteria.RawQuery;
            StateHasChanged(); // Force UI update for QueryExpressionValue
        }
#pragma warning restore CS8601

        // Renamed from SaveQuery to InvokeSave to clarify it's an event invoker
        private async Task InvokeSave()
        {
            if (string.IsNullOrWhiteSpace(QueryNameValue)) return;
            var criteria = new DataHub.Core.Models.UI.SavedCriteria { Name = QueryNameValue, RawQuery = QueryExpressionValue }; // Use UI.SavedCriteria
            await OnSave.InvokeAsync(criteria);
        }

        // Renamed from DeleteQuery to InvokeDelete to clarify it's an event invoker
        private async Task InvokeDelete()
        {
            if (string.IsNullOrWhiteSpace(QueryNameValue)) return;
            await OnDelete.InvokeAsync(QueryNameValue);
        }

        private async Task ApplyCurrentQuery()
        {
            var criteria = new DataHub.Core.Models.UI.SavedCriteria { Name = QueryNameValue, RawQuery = QueryExpressionValue }; // Use UI.SavedCriteria
            await OnApply.InvokeAsync(criteria);
            await Close();
        }

        private async Task ClearCurrentQuery()
        {
            QueryNameValue = "";
            StateHasChanged(); // Force UI update for QueryNameValue
            QueryExpressionValue = "";
            await OnClear.InvokeAsync();
            await Close();
        }

        private void InsertText(string text)
        {
            QueryExpressionValue += " " + text;
        }

        private void HandleQueryExpressionInput(ChangeEventArgs e)
        {
            QueryExpressionValue = e.Value?.ToString() ?? string.Empty;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
