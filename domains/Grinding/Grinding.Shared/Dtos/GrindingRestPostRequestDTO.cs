// Models/GrindingDto.cs
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Grinding.Shared.Dtos
{
    public class GrindingRestPostRequestDTO
    {
        public string TableName { get; set; }  = string.Empty;
        [JsonPropertyName("ID")] // Client sends "ID", maps to our ClientDbId
        public int ClientDbId { get; set; } = 0;  // Represents the ID from the client's database

        [JsonPropertyName("programName")]
        [Required(ErrorMessage = "The ProgramName field is required.")]
        public string ProgramName { get; set; } = string.Empty; 

        [JsonPropertyName("dateStart")]
        public DateTime DateStart { get; set; }

        [JsonPropertyName("grindingTime")]
        [Required(ErrorMessage = "The GrindingTime field is required.")]
        public string GrindingTime { get; set; }= string.Empty; 

        [JsonPropertyName("finishTime")]
        [Required(ErrorMessage = "The FinishTime field is required.")]
        public string FinishTime { get; set; }= string.Empty; 

        [JsonPropertyName("upperGwStart")]
        public double UpperGWStart { get; set; }= 0; 

        [JsonPropertyName("lowerGwStart")]
        public double LowerGWStart { get; set; }= 0; 

        public string Operator { get; set; }= string.Empty; 
        [Required(ErrorMessage = "The Lotto field is required.")]
        public string Lotto { get; set; }= string.Empty; 

        [JsonPropertyName("gwType")]
        [Required(ErrorMessage = "The GwType field is required.")]
        public string GwType { get; set; }= string.Empty; 
        public string ClientId { get; set; } = string.Empty; // Add ClientId
    }
    public class GrindingListRestPostRequestDTO
    {
        public List<GrindingRestPostRequestDTO> Grindings { get; set; } = new List<GrindingRestPostRequestDTO>(); // Initialize with default
    }
}
