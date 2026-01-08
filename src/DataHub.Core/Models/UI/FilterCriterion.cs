using System.Collections.Generic;

namespace DataHub.Core.Models.UI
{
    /// <summary>
    /// Represents a single filter criterion for querying data.
    /// </summary>
    public class FilterCriterion
    {
        /// <summary>
        /// The name of the field to filter on.
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// The comparison operator to use.
        /// </summary>
        public FilterOperator Operator { get; set; }

        /// <summary>
        /// The value to compare against for most operators.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// A list of values to compare against for 'In' and 'NotIn' operators.
        /// </summary>
        public List<string>? Values { get; set; }

        /// <summary>
        /// The logical operator (AND/OR) to use when combining with the next criterion.
        /// The first criterion in a list should have this set to null.
        /// </summary>
        public string? LogicalOperator { get; set; }
    }
}
