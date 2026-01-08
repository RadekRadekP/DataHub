using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataHub.Core.Models
{
    public class UserSavedCriteria
    {
        [Key]
        public int Id { get; set; } // Primary Key

        [Required]
        [MaxLength(256)] // Adjust max length as needed
        public string UserId { get; set; } = string.Empty; // To link to a user (e.g., username or user ID)

        [Required]
        [MaxLength(256)] // e.g., "DummyItems", "Alarms"
        public string TableName { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)] // User-defined name for the criteria
        public string CriteriaName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(max)")] // Store JSON string
        public string CriteriaJson { get; set; } = string.Empty; // JSON representation of SavedCriteria

        [Column(TypeName = "nvarchar(max)")] // Store raw query string
        public string? RawQuery { get; set; }
    }
}