using System.Collections.Generic;

namespace DataHub.Core.Models.UI
{
    public class ServerFilterCriterion
    {
        public string FieldName { get; set; } = string.Empty;
        public FilterOperator Operator { get; set; }
        public object? Value { get; set; }
        public string StringValue { get; set; } = string.Empty; // To hold the raw string input from UI
        public List<object>? Values { get; set; } // For 'In' and 'NotIn' operators
    }
}