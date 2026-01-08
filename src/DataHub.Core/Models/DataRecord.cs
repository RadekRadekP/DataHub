using System;
using System.ComponentModel.DataAnnotations;

namespace DataHub.Core.Models
{
    public class DataRecord
    {
        [Key]
        public int Id { get; set; } = 0; // Default for int
        public string TableName { get; set; } = string.Empty; // Initialize with default
        public string DataJson { get; set; } = string.Empty; // Initialize with default
        public string ClientId { get; set; } = string.Empty; // Initialize with default        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    }
}
