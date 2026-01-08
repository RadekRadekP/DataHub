namespace DataHub.Core.Models.UI
{
    public class SortCriterion
    {
        public string FieldName { get; set; } = string.Empty;
        public string SortDirection { get; set; } = string.Empty; // "asc" or "desc"
    }
}