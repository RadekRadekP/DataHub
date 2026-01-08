using System;
using System.Collections.Generic;

namespace RPK_BlazorApp.Models
{
    public class GrindingRestResponseDTO
    {
        public int Id { get; set; } // Server-generated primary key (from Grinding.Id)
        public int ClientDbId { get; set; } // ID from the client's database
        public string ClientId { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public DateTime DateStart { get; set; }
        public TimeSpan GrindingTime { get; set; }
        public TimeSpan FinishTime { get; set; }
        public double UpperGWStart { get; set; }
        public double LowerGWStart { get; set; }
        public string Operator { get; set; } = string.Empty;
        public string Lotto { get; set; } = string.Empty;
        public string GwType { get; set; } = string.Empty;
        public DateTime ServerTimestamp { get; set; }
        public int ChangeCounter { get; set; }

        // Optional: For consistency with request DTOs, you could add TableName
        // public string TableName { get; set; } = "Grinding";
    }

    public class GrindingListRestResponseDTO
    {
        public List<GrindingRestResponseDTO> Grindings { get; set; } = new List<GrindingRestResponseDTO>();
    }
}
