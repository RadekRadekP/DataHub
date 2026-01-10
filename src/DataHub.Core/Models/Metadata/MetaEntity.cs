using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataHub.Core.Models.Metadata
{
    /// <summary>
    /// Represents a registered entity (table/view) in the metadata catalog.
    /// </summary>
    public class MetaEntity
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The logical name of the entity (e.g., "Alarms", "VIEW_EISOD_SD")
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string EntityName { get; set; } = string.Empty;

        /// <summary>
        /// The physical table/view name in SQL Server
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// The schema name (e.g., "dbo", "export")
        /// </summary>
        [MaxLength(100)]
        public string SchemaName { get; set; } = "dbo";

        /// <summary>
        /// Which DbContext this entity belongs to (e.g., "EisodDbContext", "GrindingDbContext")
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string DbContextName { get; set; } = string.Empty;

        /// <summary>
        /// Fully qualified C# type name if available (e.g., "Eisod.Shared.Models.ViewEisodSd")
        /// </summary>
        [MaxLength(500)]
        public string? ClrTypeName { get; set; }

        /// <summary>
        /// Display name for UI (e.g., "EISOD SD View", "Alarm Records")
        /// </summary>
        [MaxLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of what this entity represents
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Is this entity a view (read-only) or a table (editable)?
        /// </summary>
        public bool IsView { get; set; }

        /// <summary>
        /// Was this entity auto-discovered or manually created?
        /// </summary>
        public bool IsDiscovered { get; set; }

        /// <summary>
        /// When this metadata entry was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this metadata entry was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
