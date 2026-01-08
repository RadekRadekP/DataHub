using System;

namespace DataHub.Core.Models.UI
{
    public class ColumnDefinition
    {
        public required string FieldName { get; set; }
        public string? DisplayName { get; set; }
        public Type? DataType { get; set; }
        public bool IsSortable { get; set; }
        public bool IsFilterable { get; set; }
    }
}