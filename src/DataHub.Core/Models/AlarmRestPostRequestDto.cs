using System;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace DataHub.Core.Models
{
    public class AlarmRestPostRequestDTO
    {
        public string TableName { get; set; }  = string.Empty;
        [JsonPropertyName("ID")] // Client sends "ID", maps to our ClientDbId
        public int ClientDbId { get; set; }  = 0; // Represents the ID from the client's database
        [JsonPropertyName("clDate")] // Add JsonPropertyName to match client JSON key
        public DateTime AlarmDate { get; set; }
        public string Type { get; set; } = string.Empty; 
        [Required]
        [JsonPropertyName("nr")] // Add JsonPropertyName to match client JSON key
        public int Nr { get; set; } = 0; 
        public string Text { get; set; } = string.Empty; 
        public string Operator { get; set; } = string.Empty; 
        // ClientId property name matches client JSON key, no JsonPropertyName needed unless case differs
        public string ClientId { get; set; } = string.Empty; // Initialize with default
    }
    public class AlarmListRestPostRequestDTO
    {
    [JsonPropertyName("Allarms")] // Aby odpovídalo klíči "Allarms" od klienta
        public List<AlarmRestPostRequestDTO> Alarms { get; set; } = new List<AlarmRestPostRequestDTO>();
    }
}
