using System.Collections.Generic;

namespace DataHub.Core.Models.UI
{
    public class DataFilterModel : DataHub.Core.Models.DataGrid.DataRequestBase
    {
        public List<FilterCriterion> Criteria { get; set; } = new List<FilterCriterion>();
        public string? LogicalOperator { get; set; }
        public string? RawQuery { get; set; }
    }
}