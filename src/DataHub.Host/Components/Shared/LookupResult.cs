namespace DataHub.Host.Components.Shared
{
    public class LookupResult
    {
        public object? SelectedId { get; set; }
        public string DisplayText { get; set; } = string.Empty;
        public Dictionary<string, object> FullRecord { get; set; } = new();
    }
}
