// Umístěte například do složky Models nebo do samostatné složky Audit
// např. RPK_BlazorApp/Models/AuditLog.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace DataHub.Core.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(256)] // Délka podle vašeho systému pro User ID
        public string UserId { get; set; } = "SYSTEM"; // Výchozí, pokud není uživatel znám

        [Required]
        [MaxLength(256)]
        public string EntityName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ActionType { get; set; } = string.Empty; // Např. "Created", "Updated", "Deleted"

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(256)] // Dostatečná délka pro primární klíče (může být i více, pokud máte složené klíče)
        public string PrimaryKey { get; set; } = string.Empty;

        public string? OldValues { get; set; } // Serializované staré hodnoty (JSON)

        public string? NewValues { get; set; } // Serializované nové hodnoty (JSON)

        public string? ChangedColumns { get; set; } // Serializovaný seznam změněných sloupců (JSON)
    }
}
