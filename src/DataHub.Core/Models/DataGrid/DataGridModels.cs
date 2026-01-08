using RPK_BlazorApp.Models.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace DataHub.Core.Models.DataGrid
{
    /// <summary>
    /// Represents a request for a paged, sorted, and filtered data set.
    /// </summary>
    public class DataResult<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }
        public int TotalUnfilteredCount { get; set; }
    }

    /// <summary>
    /// Defines the properties of a column in the GenericDataView.
    /// </summary>
    public class ColumnDefinition<TItem>
    {
        /// <summary>
        /// The name of the property in the TItem model.
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// The text to display in the column header.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the column can be sorted.
        /// </summary>
        public bool IsSortable { get; set; }

        /// <summary>
        /// Indicates if the column can be filtered.
        /// </summary>
        public bool IsFilterable { get; set; }

        /// <summary>
        /// The data type of the column, used for picking the correct filter input.
        /// </summary>
        public Type? DataType { get; set; }

        /// <summary>
        /// A function to get the value of the column from an item.
        /// </summary>
        public Expression<Func<TItem, object>> GetValue { get; set; } = default!;

        /// <summary>
        /// Indicates if the column is visible in the data grid.
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }
}