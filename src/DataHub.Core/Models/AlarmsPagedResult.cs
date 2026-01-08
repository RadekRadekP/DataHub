using System.Collections.Generic;

namespace DataHub.Core.Models
{
    public class AlarmsPagedResult
    {
        public IEnumerable<AlarmRestResponseDTO> Alarms { get; set; } = new List<AlarmRestResponseDTO>();
        public int TotalCount { get; set; }
    }
}