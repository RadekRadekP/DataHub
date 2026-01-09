using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Grinding.Shared.Dtos
{
    public class OperationalRestPostRequestDTO
    {
        // Property names should match the JSON keys sent by the client
        // Use [JsonPropertyName("...")] if JSON key differs from C# property name

        public string TableName { get; set; } = "Operational";

        [Required(ErrorMessage = "ClientId is required.")]
        public string ClientId { get; set; } = string.Empty;

        [JsonPropertyName("ID")] // This ID is expected from the client, maps to our ClientDbId property
        public int ClientDbId { get; set; } // Renamed from ID, represents the ID from the client's database

        [JsonPropertyName("clDate")] // Client sends "clDate"
        [Required(ErrorMessage = "Date is required.")]
        public DateTime Date { get; set; }

        [JsonPropertyName("eventId")] // Ensure client sends "Event ID"
        [Required(ErrorMessage = "EventId is required.")]
        public int EventId { get; set; }

        [JsonPropertyName("Description")] // Ensure client sends "Description"
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("objectName")] // JSON key from client is "objectName"
        public string? Object { get; set; } // This property is nullable

        [JsonPropertyName("Operator")] // Ensure client sends "Operator"
        [Required(ErrorMessage = "Operator is required.")]
        public string Operator { get; set; } = string.Empty;

        // ChangeCounter and ServerTimestamp are usually handled server-side, not sent in DTO
        // Server-generated fields like auto-incrementing primary keys,
        // ServerTimestamp, and ChangeCounter are typically not part of the POST request DTO
        // as they are handled by the server/database.
    }

    public class OperationalListRestPostRequestDTO
    {
        public List<OperationalRestPostRequestDTO> Operationals { get; set; } = new List<OperationalRestPostRequestDTO>();
    }
}
