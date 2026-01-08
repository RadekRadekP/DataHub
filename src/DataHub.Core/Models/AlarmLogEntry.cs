using System;

namespace DataHub.Core.Models
{
    public class AlarmLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string AlarmText { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}