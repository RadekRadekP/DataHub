using System.Collections.Generic;
using DataHub.Core.Models.UI; 
using DataHub.Core.Models;

namespace DataHub.Core.Services
{
    public class NavigationContextService
    {
        public List<object> NavigableRecordIds { get; private set; } = new List<object>();
        public string BasePath { get; private set; } = string.Empty;
        public RecordInteractionMode Mode { get; private set; } = RecordInteractionMode.Edit; // Default to Edit

        public void SetNavigationContext(List<object> recordIds, string basePath, RecordInteractionMode mode)
        {
            NavigableRecordIds = recordIds;
            BasePath = basePath;
            Mode = mode;
        }

        public void ClearNavigationContext()
        {
            NavigableRecordIds.Clear();
            BasePath = string.Empty;
            Mode = RecordInteractionMode.Edit; // Reset to default
        }
    }
}