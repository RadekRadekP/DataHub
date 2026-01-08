using System.Collections.Generic;
using RPK_BlazorApp.Models.UI; // Add this line

namespace DataHub.Core.Models.DataGrid
{
    public abstract class DataRequestBase
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public List<SortCriterion> Sorts { get; set; } = new();
        public bool GetAll { get; set; } = false;
    }
}