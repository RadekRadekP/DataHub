using System.Collections.Generic;

namespace RPK_BlazorApp.Models
{
    public class AlarmsPagedResult
    {
        public IEnumerable<AlarmRestResponseDTO> Alarms { get; set; } = new List<AlarmRestResponseDTO>();
        public int TotalCount { get; set; }
    }
}