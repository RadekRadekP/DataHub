using System.Collections.Generic;
using Grinding.Shared.Dtos;

namespace Grinding.Shared.Models
{
    public class AlarmsPagedResult
    {
        public IEnumerable<AlarmRestResponseDTO> Alarms { get; set; } = new List<AlarmRestResponseDTO>();
        public int TotalCount { get; set; }
    }
}