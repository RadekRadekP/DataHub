
using System.Collections.Generic;

namespace DataHub.Core.Models.UI
{
    public class SavedCriteria
    {
        public string Name { get; set; } = string.Empty;
        public List<FilterCriterion> Filters { get; set; } = new();
        public List<SortCriterion> Sorts { get; set; } = new();
        public string RawQuery { get; set; } = string.Empty;
    }
}
