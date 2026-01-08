using System;
using System.Collections.Generic;

namespace DataHub.Core.Models
{
    public class AlarmRestResponseDTO
    {
        public int Id { get; set; } // Server-generated primary key (from Allarm.Id)
        public int ClientDbId { get; set; } // ID from the client's database
        public string ClientId { get; set; } = string.Empty;
        public DateTime AlarmDate { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Nr { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public DateTime ServerTimestamp { get; set; }
        public int ChangeCounter { get; set; }

        // Optional: For consistency with request DTOs, you could add TableName
        // public string TableName { get; set; } = "Allarm";
    }

    public class AlarmListRestResponseDTO
    {
        public List<AlarmRestResponseDTO> Alarms { get; set; } = new List<AlarmRestResponseDTO>();
    }
}
