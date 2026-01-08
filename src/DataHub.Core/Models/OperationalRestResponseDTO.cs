using System;
using System.Collections.Generic;

namespace DataHub.Core.Models
{
    public class OperationalRestResponseDTO
    {
        public int Id { get; set; } // Server-generated primary key (from Operational.Id)
        public int ClientDbId { get; set; } // ID from the client's database
        public string ClientId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int EventId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Object { get; set; }
        public string Operator { get; set; } = string.Empty;
        public DateTime ServerTimestamp { get; set; }
        public int ChangeCounter { get; set; }

        // Optional: For consistency with request DTOs, you could add TableName
        // public string TableName { get; set; } = "Operational";
    }

    public class OperationalListRestResponseDTO
    {
        public List<OperationalRestResponseDTO> Operationals { get; set; } = new List<OperationalRestResponseDTO>();
    }
}
